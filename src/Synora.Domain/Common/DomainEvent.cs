namespace Synora.Domain.Common;

public abstract record DomainEvent
{
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
