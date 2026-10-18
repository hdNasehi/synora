using Orange.Domain.Common;
using Orange.Domain.Leave;

namespace Orange.Application.Approvals.GetInbox;

public sealed record GetInboxQuery(int Page = 1, int PageSize = 20);

public sealed record InboxItem(
    LeaveRequestId Id,
    EmployeeId EmployeeId,
    string EmployeeName,
    LeaveType Type,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Amount,
    BalanceUnit Unit,
    DateTimeOffset SubmittedAt,
    int ActiveStepOrder,
    StepKind ActiveStepKind);
