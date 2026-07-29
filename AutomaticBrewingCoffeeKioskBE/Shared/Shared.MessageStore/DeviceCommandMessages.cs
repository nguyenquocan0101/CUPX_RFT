namespace Shared.MessageStore;

public sealed record DeviceCommandRequest(
    string CommandId,
    int SchemaVersion,
    string CorrelationId,
    string WorkflowId,
    string StepId,
    string DeviceId,
    string Method,
    Dictionary<string, string> Parameters,
    DateTimeOffset RequestedAtUtc,
    int TimeoutMs);

public sealed record DeviceCommandResult(
    string CommandId,
    int SchemaVersion,
    string CorrelationId,
    string DeviceId,
    string Status,
    Dictionary<string, string> Payload,
    string? ErrorCode,
    string? ErrorMessage,
    DateTimeOffset CompletedAtUtc);
