using System.Text.Json;
using Orange.Application.Abstractions;
using Orange.Domain.Common;

namespace Orange.Infrastructure.Outbox;

public sealed class OutboxService : IOutbox
{
    private readonly IAppDbContext _db;

    public OutboxService(IAppDbContext db)
    {
        _db = db;
    }

    public void Enqueue(DomainEvent domainEvent)
    {
        _db.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = domainEvent.GetType().Name,
            Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
            CreatedAt = DateTimeOffset.UtcNow
        });
    }
}
