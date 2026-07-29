using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.MessageStore;

var options = SimulatorOptions.Parse(args);
await using var journal = new DeviceJournal(options.JournalPath);
await journal.InitializeAsync();

if (options.SelfTest)
{
    await journal.RunSelfTestAsync();
    Console.WriteLine("Device simulator journal self-test passed.");
    return;
}

var factory = new ConnectionFactory
{
    HostName = options.Host,
    Port = options.Port,
    UserName = options.UserName,
    Password = options.Password,
    DispatchConsumersAsync = true,
};

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();
channel.ExchangeDeclare(QueueConstants.EXCHANGE_DEVICE_COMMAND, ExchangeType.Direct, durable: true, autoDelete: false);
channel.ExchangeDeclare(QueueConstants.EXCHANGE_DEVICE_COMMAND + ".dlx", ExchangeType.Direct, durable: true, autoDelete: false);
channel.QueueDeclare(QueueConstants.QUEUE_DEVICE_COMMAND_DLQ, durable: true, exclusive: false, autoDelete: false);
channel.QueueBind(QueueConstants.QUEUE_DEVICE_COMMAND_DLQ, QueueConstants.EXCHANGE_DEVICE_COMMAND + ".dlx", QueueConstants.ROUTING_DEVICE_COMMAND);
var arguments = new Dictionary<string, object>
{
    ["x-dead-letter-exchange"] = QueueConstants.EXCHANGE_DEVICE_COMMAND + ".dlx",
    ["x-dead-letter-routing-key"] = QueueConstants.ROUTING_DEVICE_COMMAND,
};
channel.QueueDeclare(QueueConstants.QUEUE_DEVICE_COMMAND, durable: true, exclusive: false, autoDelete: false, arguments);
channel.QueueBind(QueueConstants.QUEUE_DEVICE_COMMAND, QueueConstants.EXCHANGE_DEVICE_COMMAND, QueueConstants.ROUTING_DEVICE_COMMAND);
channel.BasicQos(0, 1, false);

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.Received += async (_, eventArgs) =>
{
    try
    {
        var request = JsonSerializer.Deserialize<DeviceCommandRequest>(eventArgs.Body.Span)
            ?? throw new InvalidDataException("Command payload is empty.");
        var result = await journal.ExecuteAsync(request, options);
        Console.WriteLine($"{request.CommandId}: {result.Status}");
        var replyTo = eventArgs.BasicProperties?.ReplyTo;
        if (!string.IsNullOrWhiteSpace(replyTo))
        {
            var resultProperties = channel.CreateBasicProperties();
            resultProperties.Persistent = true;
            resultProperties.ContentType = "application/json";
            resultProperties.CorrelationId = request.CommandId;
            channel.BasicPublish(
                exchange: string.Empty,
                routingKey: replyTo,
                mandatory: false,
                basicProperties: resultProperties,
                body: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result)));
        }
        channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
    }
    catch (Exception error)
    {
        Console.Error.WriteLine($"Device command rejected: {error.Message}");
        channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
    }
};
channel.BasicConsume(QueueConstants.QUEUE_DEVICE_COMMAND, autoAck: false, consumer);
Console.WriteLine($"Device simulator listening on {QueueConstants.QUEUE_DEVICE_COMMAND}. Press Ctrl+C to stop.");
await Task.Delay(Timeout.InfiniteTimeSpan);

sealed record SimulatorOptions(
    string Host,
    int Port,
    string UserName,
    string Password,
    string JournalPath,
    bool SelfTest,
    int DelayMs,
    string? FailMethod)
{
    public static SimulatorOptions Parse(string[] args)
    {
        string Value(string name, string fallback) =>
            args.FirstOrDefault(x => x.StartsWith(name + "=", StringComparison.OrdinalIgnoreCase))?.Split('=', 2)[1] ?? fallback;

        return new(
            Value("--host", Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost"),
            int.Parse(Value("--port", Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672")),
            Value("--user", Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "cupx"),
            Value("--password", Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "cupx123456"),
            Value("--journal", Path.Combine(AppContext.BaseDirectory, "device-simulator.db")),
            args.Contains("--self-test", StringComparer.OrdinalIgnoreCase),
            int.Parse(Value("--delay-ms", "50")),
            Value("--fail-method", "") is { Length: > 0 } fail ? fail : null);
    }
}

sealed class DeviceJournal : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    public DeviceJournal(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        _connection = new SqliteConnection($"Data Source={path}");
    }

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
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
        await command.ExecuteNonQueryAsync();
    }

    public async Task<DeviceCommandResult> ExecuteAsync(DeviceCommandRequest request, SimulatorOptions options)
    {
        var existing = await FindAsync(request.CommandId);
        if (existing is not null)
        {
            var stored = existing.Value;
            if (stored.Status is "Completed" or "Failed")
                return JsonSerializer.Deserialize<DeviceCommandResult>(stored.ResultJson!)!;
            if (stored.Status == "Unknown")
                throw new InvalidOperationException("Command is Unknown and requires explicit reconciliation.");
        }

        await SaveAsync(request, "Received", null);
        await SaveAsync(request, "Executing", null);
        await Task.Delay(Math.Max(0, options.DelayMs));

        var failed = options.FailMethod is not null && string.Equals(options.FailMethod, request.Method, StringComparison.OrdinalIgnoreCase);
        var result = new DeviceCommandResult(
            request.CommandId, request.SchemaVersion, request.CorrelationId, request.DeviceId,
            failed ? "Failed" : "Completed",
            failed ? new() : new() { ["method"] = request.Method, ["simulated"] = "true" },
            failed ? "SIMULATED_FAILURE" : null,
            failed ? "The simulator was configured to fail this method." : null,
            DateTimeOffset.UtcNow);
        await SaveAsync(request, result.Status, JsonSerializer.Serialize(result));
        return result;
    }

    public async Task RunSelfTestAsync()
    {
        var request = new DeviceCommandRequest("self-test-command", 1, "self-test-correlation", "workflow", "step", "device", "dispense", new(), DateTimeOffset.UtcNow, 1000);
        var first = await ExecuteAsync(request, SimulatorOptions.Parse(Array.Empty<string>()));
        var second = await ExecuteAsync(request, SimulatorOptions.Parse(Array.Empty<string>()));
        if (first.Status != "Completed" || second.Status != "Completed") throw new InvalidOperationException("Idempotency check failed.");
    }

    private async Task<(string Status, string? ResultJson)?> FindAsync(string commandId)
    {
        await using var command = _connection.CreateCommand();
        command.CommandText = "SELECT Status, ResultJson FROM DeviceCommands WHERE CommandId = $id";
        command.Parameters.AddWithValue("$id", commandId);
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? (reader.GetString(0), reader.IsDBNull(1) ? null : reader.GetString(1)) : null;
    }

    private async Task SaveAsync(DeviceCommandRequest request, string status, string? resultJson)
    {
        await using var command = _connection.CreateCommand();
        command.CommandText = """
            INSERT INTO DeviceCommands(CommandId, CorrelationId, DeviceId, Status, ResultJson, UpdatedAtUtc)
            VALUES ($id, $correlation, $device, $status, $result, $now)
            ON CONFLICT(CommandId) DO UPDATE SET Status = excluded.Status, ResultJson = excluded.ResultJson, UpdatedAtUtc = excluded.UpdatedAtUtc;
            """;
        command.Parameters.AddWithValue("$id", request.CommandId);
        command.Parameters.AddWithValue("$correlation", request.CorrelationId);
        command.Parameters.AddWithValue("$device", request.DeviceId);
        command.Parameters.AddWithValue("$status", status);
        command.Parameters.AddWithValue("$result", (object?)resultJson ?? DBNull.Value);
        command.Parameters.AddWithValue("$now", DateTimeOffset.UtcNow.ToString("O"));
        await command.ExecuteNonQueryAsync();
    }

    public ValueTask DisposeAsync() => _connection.DisposeAsync();
}
