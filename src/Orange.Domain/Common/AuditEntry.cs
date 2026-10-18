namespace Orange.Domain.Common;

public sealed class AuditEntry
{
    public AuditEntryId Id { get; init; }
    public EmployeeId ActorId { get; init; }
    public string Action { get; init; } = "";
    public string EntityType { get; init; } = "";
    public string? EntityId { get; init; }
    public string Details { get; init; } = "";
    public DateTimeOffset Timestamp { get; init; }
}
