using Orange.Domain.Common;

namespace Orange.Application.Approvals.Approve;

public sealed record ApproveRequestCommand(
    LeaveRequestId RequestId,
    string? Comment,
    EmployeeId? OnBehalfOf);
