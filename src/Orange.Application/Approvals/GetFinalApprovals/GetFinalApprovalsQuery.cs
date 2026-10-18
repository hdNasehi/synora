using Orange.Domain.Common;
using Orange.Domain.Leave;

namespace Orange.Application.Approvals.GetFinalApprovals;

public sealed record GetFinalApprovalsQuery(int Page = 1, int PageSize = 20);

public sealed record FinalApprovalItem(
    LeaveRequestId Id,
    EmployeeId EmployeeId,
    string EmployeeName,
    LeaveType Type,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Amount,
    BalanceUnit Unit,
    DateTimeOffset SubmittedAt);
