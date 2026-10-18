using Orange.Domain.Common;

namespace Orange.Application.Requests.Cancel;

public sealed record CancelLeaveRequest(LeaveRequestId RequestId, string Reason);
