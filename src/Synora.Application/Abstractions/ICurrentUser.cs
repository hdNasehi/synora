using Synora.Domain.Common;

namespace Synora.Application.Abstractions;

public interface ICurrentUser
{
    EmployeeId EmployeeId { get; }
    IReadOnlySet<AppRole> Roles { get; }
    bool IsInRole(AppRole role);
}
