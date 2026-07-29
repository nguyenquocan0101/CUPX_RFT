using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.MessageStore;

namespace Shared.RabbitMqPublisher;

public static class LocalDeviceCommandPayload
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static string ToJson(IReadOnlyDictionary<string, string> parameters)
    {
        if (parameters.TryGetValue("raw", out var raw) && !string.IsNullOrWhiteSpace(raw))
            return raw;

        var values = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in parameters)
        {
            try
            {
                using var document = JsonDocument.Parse(pair.Value);
                values[pair.Key] = document.RootElement.Clone();
            }
            catch (JsonException)
            {
                values[pair.Key] = JsonSerializer.SerializeToElement(pair.Value, JsonOptions);
            }
        }

        return JsonSerializer.Serialize(values, JsonOptions);
    }
}

public sealed class LocalDeviceCommandHostOptions
{
    public string HostName { get; init; } = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
    public int Port { get; init; } = ParseInt("RABBITMQ_PORT", 5672);
    public string UserName { get; init; } = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "guest";
    public string Password { get; init; } = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest";
    public string JournalPath { get; init; } = Path.Combine(AppContext.BaseDirectory, "device-command.db");

    private static int ParseInt(string name, int fallback) =>
        int.TryParse(Environment.GetEnvironmentVariable(name), out var value) ? value : fallback;
}

public sealed class LocalDeviceCommandHost : IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly string _deviceId;
    private readonly Func<DeviceCommandRequest, CancellationToken, Task<DeviceCommandResult>> _handler;
    private readonly LocalDeviceCommandHostOptions _options;
    private readonly LocalDeviceCommandJournal _journal;
    private IConnection? _connection;
    private IChannel? _channel;
    private string? _queueName;

    public LocalDeviceCommandHost(
        string deviceId,
        Func<DeviceCommandRequest, CancellationToken, Task<DeviceCommandResult>> handler,
        LocalDeviceCommandHostOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("A real controller requires DEVICE_ID.", nameof(deviceId));

        _deviceId = deviceId;
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        _options = options ?? new LocalDeviceCommandHostOptions();
        _journal = new LocalDeviceCommandJournal(_options.JournalPath);
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _journal.InitializeAsync(cancellationToken);
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
        };
        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.ExchangeDeclareAsync(
            QueueConstants.EXCHANGE_DEVICE_COMMAND,
            ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);
        await _channel.ExchangeDeclareAsync(
            QueueConstants.EXCHANGE_DEVICE_COMMAND + ".dlx",
            ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);
        await _channel.QueueDeclareAsync(
            QueueConstants.QUEUE_DEVICE_COMMAND_DLQ,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);
        await _channel.QueueBindAsync(
            QueueConstants.QUEUE_DEVICE_COMMAND_DLQ,
            QueueConstants.EXCHANGE_DEVICE_COMMAND + ".dlx",
            QueueConstants.ROUTING_DEVICE_COMMAND,
            cancellationToken: cancellationToken);

        _queueName = $"device-command.{SanitizeQueuePart(_deviceId)}";
        var queueArguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = QueueConstants.EXCHANGE_DEVICE_COMMAND + ".dlx",
            ["x-dead-letter-routing-key"] = QueueConstants.ROUTING_DEVICE_COMMAND,
        };
        await _channel.QueueDeclareAsync(
            _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArguments,
            cancellationToken: cancellationToken);
        await _channel.QueueBindAsync(
            _queueName,
            QueueConstants.EXCHANGE_DEVICE_COMMAND,
            QueueConstants.ROUTING_DEVICE_COMMAND,
            cancellationToken: cancellationToken);
        await _channel.BasicQosAsync(0, 1, global: false, cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += HandleDeliveryAsync;
        await _channel.BasicConsumeAsync(_queueName, autoAck: false, consumer, cancellationToken);
        Console.WriteLine($"Local RabbitMQ controller listening: device={_deviceId} queue={_queueName}");
    }

    private async Task HandleDeliveryAsync(object sender, BasicDeliverEventArgs delivery)
    {
        if (_channel is null)
            return;

        DeviceCommandRequest? request;
        try
        {
            request = JsonSerializer.Deserialize<DeviceCommandRequest>(delivery.Body.Span, JsonOptions);
            if (request is null)
                throw new InvalidDataException("Device command payload is empty.");
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"Rejected malformed device command: {error.Message}");
            await _channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: false);
            return;
        }

        if (!string.Equals(request.DeviceId, _deviceId, StringComparison.Ordinal))
        {
            // Each real controller has its own queue bound to the shared route.
            await _channel.BasicAckAsync(delivery.DeliveryTag, multiple: false);
            return;
        }

        DeviceCommandResult result;
        try
        {
            var replay = await _journal.TryClaimAsync(request, CancellationToken.None);
            result = replay is not null
                ? JsonSerializer.Deserialize<DeviceCommandResult>(replay, JsonOptions)!
                : await _handler(request, CancellationToken.None);

            if (replay is null)
                await _journal.SaveResultAsync(request, result, CancellationToken.None);
        }
        catch (InvalidOperationException error) when (error.Message.Contains("uncertain outcome", StringComparison.Ordinal))
        {
            result = Failure(request, "DEVICE_UNKNOWN_OUTCOME", error.Message);
        }
        catch (Exception error)
        {
            result = Failure(request, "DEVICE_HANDLER_FAILURE", error.Message);
            await _journal.SaveResultAsync(request, result, CancellationToken.None);
        }

        try
        {
            if (!string.IsNullOrWhiteSpace(delivery.BasicProperties?.ReplyTo))
            {
                var properties = new BasicProperties
                {
                    Persistent = true,
                    ContentType = "application/json",
                    CorrelationId = request.CommandId,
                };
                await _channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: delivery.BasicProperties.ReplyTo,
                    mandatory: false,
                    basicProperties: properties,
                    body: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result, JsonOptions)));
            }
        }
        finally
        {
            // The journal is durable; ACK after the handler/result is recorded so a
            // lost reply can be recovered by replaying the same command ID.
            await _channel.BasicAckAsync(delivery.DeliveryTag, multiple: false);
        }
    }

    private static DeviceCommandResult Failure(DeviceCommandRequest request, string code, string message) =>
        new(
            request.CommandId,
            request.SchemaVersion,
            request.CorrelationId,
            request.DeviceId,
            "Failed",
            new Dictionary<string, string>(),
            code,
            message.Length > 2000 ? message[..2000] : message,
            DateTimeOffset.UtcNow);

    private static string SanitizeQueuePart(string value) =>
        new(value.Select(character => char.IsLetterOrDigit(character) || character is '-' or '_' ? character : '_').ToArray());

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
            await _channel.DisposeAsync();
        if (_connection is not null)
            await _connection.DisposeAsync();
        await _journal.DisposeAsync();
    }
}

internal sealed class LocalDeviceCommandJournal : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public LocalDeviceCommandJournal(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        _connection = new SqliteConnection($"Data Source={path};Default Timeout=5");
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await _connection.OpenAsync(cancellationToken);
        await using var command = _connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS DeviceCommands (
                CommandId TEXT PRIMARY KEY,
                CorrelationId TEXT NOT NULL,
                DeviceId TEXT NOT NULL,
                Status TEXT NOT NULL,
                ResultJson TEXT NULL,
                UpdatedAtUtc TEXT NOT NULL
            );
            UPDATE DeviceCommands SET Status = 'Unknown', UpdatedAtUtc = $now WHERE Status = 'Executing';
            """;
        command.Parameters.AddWithValue("$now", DateTimeOffset.UtcNow.ToString("O"));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<string?> TryClaimAsync(DeviceCommandRequest request, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            await using var transaction = _connection.BeginTransaction();
            await using var insert = _connection.CreateCommand();
            insert.Transaction = transaction;
            insert.CommandText = """
                INSERT INTO DeviceCommands(CommandId, CorrelationId, DeviceId, Status, ResultJson, UpdatedAtUtc)
                VALUES ($id, $correlation, $device, 'Received', NULL, $now)
                ON CONFLICT(CommandId) DO NOTHING;
                """;
            insert.Parameters.AddWithValue("$id", request.CommandId);
            insert.Parameters.AddWithValue("$correlation", request.CorrelationId);
            insert.Parameters.AddWithValue("$device", request.DeviceId);
            insert.Parameters.AddWithValue("$now", DateTimeOffset.UtcNow.ToString("O"));
            await insert.ExecuteNonQueryAsync(cancellationToken);

            await using var claim = _connection.CreateCommand();
            claim.Transaction = transaction;
            claim.CommandText = "UPDATE DeviceCommands SET Status = 'Executing', UpdatedAtUtc = $now WHERE CommandId = $id AND Status = 'Received'";
            claim.Parameters.AddWithValue("$id", request.CommandId);
            claim.Parameters.AddWithValue("$now", DateTimeOffset.UtcNow.ToString("O"));
            if (await claim.ExecuteNonQueryAsync(cancellationToken) == 0)
            {
                await using var existing = _connection.CreateCommand();
                existing.Transaction = transaction;
                existing.CommandText = "SELECT Status, ResultJson FROM DeviceCommands WHERE CommandId = $id";
                existing.Parameters.AddWithValue("$id", request.CommandId);
                await using var reader = await existing.ExecuteReaderAsync(cancellationToken);
                if (!await reader.ReadAsync(cancellationToken))
                    throw new InvalidOperationException($"Command {request.CommandId} disappeared from the journal.");
                var status = reader.GetString(0);
                var result = reader.IsDBNull(1) ? null : reader.GetString(1);
                if (status is "Completed" or "Failed")
                {
                    await reader.DisposeAsync();
                    await transaction.CommitAsync(cancellationToken);
                    return result ?? throw new InvalidOperationException($"Command {request.CommandId} has no stored result.");
                }

                throw new InvalidOperationException("Command has an uncertain outcome and requires explicit reconciliation.");
            }

            await transaction.CommitAsync(cancellationToken);
            return null;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task SaveResultAsync(DeviceCommandRequest request, DeviceCommandResult result, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            await using var command = _connection.CreateCommand();
            command.CommandText = "UPDATE DeviceCommands SET Status = $status, ResultJson = $result, UpdatedAtUtc = $now WHERE CommandId = $id";
            command.Parameters.AddWithValue("$id", request.CommandId);
            command.Parameters.AddWithValue("$status", result.Status);
            command.Parameters.AddWithValue("$result", JsonSerializer.Serialize(result, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
            command.Parameters.AddWithValue("$now", DateTimeOffset.UtcNow.ToString("O"));
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
        _gate.Dispose();
    }
}
