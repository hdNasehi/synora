using Synora.Domain.Common;

namespace Synora.Application.Abstractions;

public interface IAuditLog
{
    void Record(string action, object details, EmployeeId actorId);
}
