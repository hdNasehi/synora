using Orange.Domain.Common;
using Orange.Domain.Employees;
using Orange.Domain.Leave;

namespace Orange.Domain.Services;

public interface IApprovalRouteBuilder
{
    Result<IReadOnlyList<ApprovalStep>> Build(
        EmployeeAssignment assignment,
        LeaveType type);
}
