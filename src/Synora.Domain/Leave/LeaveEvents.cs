using Synora.Domain.Common;

namespace Synora.Domain.Leave;

public sealed record LeaveRequestSubmitted(
    LeaveRequestId RequestId,
    EmployeeId EmployeeId,
    LeaveType Type) : DomainEvent;

public sealed record LeaveRequestApproved(
    LeaveRequestId RequestId,
    EmployeeId EmployeeId) : DomainEvent;

public sealed record LeaveRequestRejected(
    LeaveRequestId RequestId,
    EmployeeId EmployeeId,
    string Reason) : DomainEvent;

public sealed record LeaveRequestCancelled(
    LeaveRequestId RequestId,
    EmployeeId EmployeeId) : DomainEvent;

public sealed record ApprovalStepCompleted(
    LeaveRequestId RequestId,
    ApprovalStepId StepId,
    StepKind Kind) : DomainEvent;
