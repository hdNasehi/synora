using Orange.Domain.Common;
using Orange.Domain.Leave;

namespace Orange.Application.Requests.Submit;

public sealed record SubmitLeaveRequest(
    LeaveType Type,
    DateOnly StartDate,
    DateOnly EndDate,
    TimeOnly? PartialFrom,
    TimeOnly? PartialTo,
    string? Reason);

public sealed record SubmitLeaveResponse(
    LeaveRequestId Id,
    RequestStatus Status,
    decimal Amount,
    BalanceUnit Unit,
    LeaveCalculation Calculation);
