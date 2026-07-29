using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.MessageStore;

var options = SimulatorOptions.Parse(args);
await using var journal = new DeviceJournal(options.JournalPath);
await journal.InitializeAsync();

if (!string.IsNullOrWhiteSpace(options.ReconcileCommandId))
{
    await journal.ReconcileAsync(options.ReconcileCommandId, options.ReconcileResolution);
    Console.WriteLine($"Device command reconciled: {options.ReconcileCommandId} resolution={options.ReconcileResolution}");
    return;
}

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
    string? FailMethod,
    string? ReconcileCommandId,
    string ReconcileResolution)
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
            Value("--fail-method", "") is { Length: > 0 } fail ? fail : null,
            Value("--reconcile", "") is { Length: > 0 } commandId ? commandId : null,
            Value("--resolution", "Failed"));
    }
}

sealed class DeviceJournal : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SemaphoreSlim _databaseGate = new(1, 1);

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
            CREATE TABLE IF NOT EXISTS DeviceCommandReconciliations (
                CommandId TEXT PRIMARY KEY,
                Resolution TEXT NOT NULL,
                Note TEXT NOT NULL,
                ReconciledAtUtc TEXT NOT NULL
            );
            UPDATE DeviceCommands SET Status = 'Unknown', UpdatedAtUtc = $now WHERE Status = 'Executing';
            """;
        command.Parameters.AddWithValue("$now", DateTimeOffset.UtcNow.ToString("O"));
        await command.ExecuteNonQueryAsync();
    }

    public async Task<DeviceCommandResult> ExecuteAsync(DeviceCommandRequest request, SimulatorOptions options)
    {
        var replay = await ClaimExecutionAsync(request);
        if (replay is not null)
            return JsonSerializer.Deserialize<DeviceCommandResult>(replay)!;

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

        var concurrent = request with { CommandId = "self-test-concurrent-command", CorrelationId = "self-test-concurrent-correlation" };
        var concurrentResults = await Task.WhenAll(Enumerable.Range(0, 2).Select(async _ =>
        {
            try
            {
                return await ExecuteAsync(concurrent, SimulatorOptions.Parse(Array.Empty<string>()));
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }));
        if (concurrentResults.Count(x => x?.Status == "Completed") != 1)
            throw new InvalidOperationException("Concurrent command claim check failed.");

        var unknown = request with { CommandId = "self-test-unknown-command", CorrelationId = "self-test-unknown-correlation" };
        await SaveAsync(unknown, "Executing", null);
        await MarkUnknownAsync(unknown.CommandId);
        try
        {
            await ExecuteAsync(unknown, SimulatorOptions.Parse(Array.Empty<string>()));
            throw new InvalidOperationException("Unknown command was executed without reconciliation.");
        }
        catch (InvalidOperationException error) when (error.Message.Contains("requires explicit reconciliation", StringComparison.Ordinal))
        {
            // Expected: an unknown physical outcome must be resolved by an operator.
        }

        await ReconcileAsync(unknown.CommandId, "Failed");
        var reconciled = await ExecuteAsync(unknown, SimulatorOptions.Parse(Array.Empty<string>()));
        if (reconciled.Status != "Failed" || reconciled.ErrorCode != "OPERATOR_RECONCILED")
            throw new InvalidOperationException("Unknown command reconciliation check failed.");
    }

    public async Task ReconcileAsync(string commandId, string resolution)
    {
        await _databaseGate.WaitAsync();
        try
        {
            await ReconcileCoreAsync(commandId, resolution);
        }
        finally
        {
            _databaseGate.Release();
        }
    }

    private async Task ReconcileCoreAsync(string commandId, string resolution)
    {
        if (resolution is not ("Completed" or "Failed"))
            throw new ArgumentException("Resolution must be Completed or Failed.", nameof(resolution));

        await using var transaction = _connection.BeginTransaction();
        await using var find = _connection.CreateCommand();
        find.Transaction = transaction;
        find.CommandText = "SELECT CorrelationId, DeviceId, Status FROM DeviceCommands WHERE CommandId = $id";
        find.Parameters.AddWithValue("$id", commandId);
        await using var reader = await find.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            throw new InvalidOperationException($"Command {commandId} was not found in the journal.");
        var correlationId = reader.GetString(0);
        var deviceId = reader.GetString(1);
        var status = reader.GetString(2);
        await reader.DisposeAsync();
        if (status != "Unknown")
            throw new InvalidOperationException($"Command {commandId} is {status}; only Unknown commands can be reconciled.");

        var result = new DeviceCommandResult(
            commandId,
            1,
            correlationId,
            deviceId,
            resolution,
            new() { ["reconciled"] = "true" },
            "OPERATOR_RECONCILED",
            "Physical outcome was explicitly reconciled by an operator.",
            DateTimeOffset.UtcNow);
        await using var update = _connection.CreateCommand();
        update.Transaction = transaction;
        update.CommandText = "UPDATE DeviceCommands SET Status = $status, ResultJson = $result, UpdatedAtUtc = $now WHERE CommandId = $id";
        update.Parameters.AddWithValue("$status", resolution);
        update.Parameters.AddWithValue("$result", JsonSerializer.Serialize(result));
        update.Parameters.AddWithValue("$now", DateTimeOffset.UtcNow.ToString("O"));
        update.Parameters.AddWithValue("$id", commandId);
        await update.ExecuteNonQueryAsync();

        await using var audit = _connection.CreateCommand();
        audit.Transaction = transaction;
        audit.CommandText = """
            INSERT INTO DeviceCommandReconciliations(CommandId, Resolution, Note, ReconciledAtUtc)
            VALUES ($id, $resolution, $note, $now)
            ON CONFLICT(CommandId) DO UPDATE SET Resolution = excluded.Resolution, Note = excluded.Note, ReconciledAtUtc = excluded.ReconciledAtUtc;
            """;
        audit.Parameters.AddWithValue("$id", commandId);
        audit.Parameters.AddWithValue("$resolution", resolution);
        audit.Parameters.AddWithValue("$note", "Physical outcome explicitly reconciled by operator.");
        audit.Parameters.AddWithValue("$now", DateTimeOffset.UtcNow.ToString("O"));
        await audit.ExecuteNonQueryAsync();
        await transaction.CommitAsync();
    }

    private async Task MarkUnknownAsync(string commandId)
    {
        await _databaseGate.WaitAsync();
        try
        {
            await using var command = _connection.CreateCommand();
            command.CommandText = "UPDATE DeviceCommands SET Status = 'Unknown', UpdatedAtUtc = $now WHERE CommandId = $id";
            command.Parameters.AddWithValue("$id", commandId);
            command.Parameters.AddWithValue("$now", DateTimeOffset.UtcNow.ToString("O"));
            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            _databaseGate.Release();
        }
    }

    private async Task<string?> ClaimExecutionAsync(DeviceCommandRequest request)
    {
        await _databaseGate.WaitAsync();
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
            await insert.ExecuteNonQueryAsync();

            await using var claim = _connection.CreateCommand();
            claim.Transaction = transaction;
            claim.CommandText = "UPDATE DeviceCommands SET Status = 'Executing', UpdatedAtUtc = $now WHERE CommandId = $id AND Status = 'Received'";
            claim.Parameters.AddWithValue("$id", request.CommandId);
            claim.Parameters.AddWithValue("$now", DateTimeOffset.UtcNow.ToString("O"));
            var claimed = await claim.ExecuteNonQueryAsync();
            if (claimed == 0)
            {
                await using var existing = _connection.CreateCommand();
                existing.Transaction = transaction;
                existing.CommandText = "SELECT Status, ResultJson FROM DeviceCommands WHERE CommandId = $id";
                existing.Parameters.AddWithValue("$id", request.CommandId);
                await using var reader = await existing.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                    throw new InvalidOperationException($"Command {request.CommandId} disappeared from the journal.");
                var status = reader.GetString(0);
                var resultJson = reader.IsDBNull(1) ? null : reader.GetString(1);
                if (status is "Completed" or "Failed")
                {
                    await transaction.CommitAsync();
                    return resultJson ?? throw new InvalidOperationException($"Command {request.CommandId} has no stored result.");
                }

                throw new InvalidOperationException("Command has an uncertain outcome and requires explicit reconciliation.");
            }

            await transaction.CommitAsync();
            return null;
        }
        finally
        {
            _databaseGate.Release();
        }
    }

    private async Task SaveAsync(DeviceCommandRequest request, string status, string? resultJson)
    {
        await _databaseGate.WaitAsync();
        try
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
        finally
        {
            _databaseGate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
        _databaseGate.Dispose();
    }
}
