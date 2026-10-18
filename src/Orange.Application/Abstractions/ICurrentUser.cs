using Orange.Domain.Common;

namespace Orange.Application.Abstractions;

public interface ICurrentUser
{
    EmployeeId EmployeeId { get; }
    IReadOnlySet<AppRole> Roles { get; }
    bool IsInRole(AppRole role);
}
