using System.Text.Json;
using Orange.Application.Abstractions;
using Orange.Domain.Common;

namespace Orange.Infrastructure.Audit;

public sealed class AuditLogService : IAuditLog
{
    private readonly IAppDbContext _db;

    public AuditLogService(IAppDbContext db)
    {
        _db = db;
    }

    public void Record(string action, object details, EmployeeId actorId)
    {
        _db.AuditEntries.Add(new AuditEntry
        {
            Id = new AuditEntryId(Guid.NewGuid()),
            ActorId = actorId,
            Action = action,
            EntityType = "",
            Details = JsonSerializer.Serialize(details),
            Timestamp = DateTimeOffset.UtcNow
        });
    }
}
