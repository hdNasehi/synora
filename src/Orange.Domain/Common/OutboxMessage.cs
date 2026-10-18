namespace Orange.Domain.Common;

public sealed class OutboxMessage
{
    public Guid Id { get; init; }
    public string EventType { get; init; } = "";
    public string Payload { get; init; } = "";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public int RetryCount { get; set; }
    public string? LastError { get; set; }
}
