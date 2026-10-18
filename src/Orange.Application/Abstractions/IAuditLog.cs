using Orange.Domain.Common;

namespace Orange.Application.Abstractions;

public interface IAuditLog
{
    void Record(string action, object details, EmployeeId actorId);
}
