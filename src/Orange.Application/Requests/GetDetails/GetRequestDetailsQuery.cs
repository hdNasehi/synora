using Orange.Domain.Common;
using Orange.Domain.Leave;

namespace Orange.Application.Requests.GetDetails;

public sealed record GetRequestDetailsQuery(LeaveRequestId RequestId);

public sealed record LeaveRequestDetail(
    LeaveRequestId Id,
    EmployeeId EmployeeId,
    string EmployeeName,
    LeaveType Type,
    DateOnly StartDate,
    DateOnly EndDate,
    TimeRange? PartialTime,
    decimal Amount,
    BalanceUnit Unit,
    string? Reason,
    RequestStatus Status,
    LeaveCalculation Calculation,
    IReadOnlyList<ApprovalStepDetail> Steps,
    DateTimeOffset SubmittedAt);

public sealed record ApprovalStepDetail(
    ApprovalStepId Id,
    int Order,
    StepKind Kind,
    ApprovalRule Rule,
    StepState State,
    IReadOnlyList<EmployeeId> ApproverIds,
    IReadOnlyList<ApprovalDecisionDetail> Decisions);

public sealed record ApprovalDecisionDetail(
    EmployeeId DecidedBy,
    DecisionType Decision,
    DateTimeOffset DecidedAt,
    string? Comment,
    EmployeeId? OnBehalfOf);
