using Synora.Domain.Common;
using Synora.Domain.Employees;
using Synora.Domain.Leave;

namespace Synora.Domain.Services;

public interface IApprovalRouteBuilder
{
    Result<IReadOnlyList<ApprovalStep>> Build(
        EmployeeAssignment assignment,
        LeaveType type);
}
