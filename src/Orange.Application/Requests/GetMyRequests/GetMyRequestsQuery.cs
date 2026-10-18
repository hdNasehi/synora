using Orange.Application.Common;
using Orange.Domain.Common;
using Orange.Domain.Leave;

namespace Orange.Application.Requests.GetMyRequests;

public sealed record GetMyRequestsQuery(
    int Page = 1,
    int PageSize = 20,
    RequestStatus? Status = null,
    int? Year = null);

public sealed record LeaveRequestSummary(
    LeaveRequestId Id,
    LeaveType Type,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Amount,
    BalanceUnit Unit,
    RequestStatus Status,
    DateTimeOffset SubmittedAt);
