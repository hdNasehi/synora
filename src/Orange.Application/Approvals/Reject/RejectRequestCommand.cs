using Orange.Domain.Common;

namespace Orange.Application.Approvals.Reject;

public sealed record RejectRequestCommand(
    LeaveRequestId RequestId,
    string Reason,
    EmployeeId? OnBehalfOf);
