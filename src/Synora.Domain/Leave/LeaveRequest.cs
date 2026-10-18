using Synora.Domain.Common;

namespace Synora.Domain.Leave;

public sealed class LeaveRequest : AggregateRoot<LeaveRequestId>
{
    private readonly List<ApprovalStep> _steps = [];

    public EmployeeId EmployeeId { get; private set; }
    public LeaveType Type { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public TimeRange? PartialTime { get; private set; }
    public decimal Amount { get; private set; }
    public BalanceUnit Unit { get; private set; }
    public string? Reason { get; private set; }
    public LeaveCalculation Calculation { get; private set; } = null!;
    public DateTimeOffset SubmittedAt { get; private set; }
    public IReadOnlyList<ApprovalStep> Steps => _steps.AsReadOnly();
    public byte[] RowVersion { get; private set; } = [];

    public RequestStatus Status => DeriveStatus();

    private LeaveRequest() { }

    public static LeaveRequest Submit(
        EmployeeId employeeId,
        LeaveType type,
        DateOnly startDate,
        DateOnly endDate,
        TimeRange? partialTime,
        decimal amount,
        BalanceUnit unit,
        string? reason,
        LeaveCalculation calculation,
        IReadOnlyList<ApprovalStep> steps,
        DateTimeOffset submittedAt)
    {
        var request = new LeaveRequest
        {
            Id = new LeaveRequestId(Guid.NewGuid()),
            EmployeeId = employeeId,
            Type = type,
            StartDate = startDate,
            EndDate = endDate,
            PartialTime = partialTime,
            Amount = amount,
            Unit = unit,
            Reason = reason,
            Calculation = calculation,
            SubmittedAt = submittedAt
        };

        request._steps.AddRange(steps);
        request.RaiseDomainEvent(new LeaveRequestSubmitted(request.Id, employeeId, type));

        return request;
    }

    public Result<RequestStatus> Approve(
        EmployeeId actorId, string? comment,
        EmployeeId? onBehalfOf, DateTimeOffset decidedAt)
    {
        if (EmployeeId == actorId)
            return DomainErrors.CannotApproveOwnRequest;

        var activeStep = _steps.FirstOrDefault(s => s.State == StepState.Active);
        if (activeStep is null)
            return DomainErrors.NotAuthorizedToAct;

        var targetApprover = onBehalfOf ?? actorId;
        if (!activeStep.ApproverIds.Contains(targetApprover))
            return DomainErrors.NotAuthorizedToAct;

        activeStep.AddDecision(new ApprovalDecision(actorId, DecisionType.Approved, decidedAt, comment, onBehalfOf));

        var approvedBy = activeStep.Decisions
            .Where(d => d.Decision == DecisionType.Approved)
            .Select(d => d.OnBehalfOf ?? d.DecidedBy)
            .ToHashSet();

        var stepComplete = activeStep.Rule == ApprovalRule.All
            ? activeStep.ApproverIds.All(a => approvedBy.Contains(a))
            : true;

        if (stepComplete)
        {
            activeStep.State = StepState.Done;
            RaiseDomainEvent(new ApprovalStepCompleted(Id, activeStep.Id, activeStep.Kind));

            var nextStep = _steps.FirstOrDefault(s => s.State == StepState.Waiting);
            if (nextStep is not null)
                nextStep.State = StepState.Active;
        }

        var newStatus = DeriveStatus();
        if (newStatus == RequestStatus.Approved)
            RaiseDomainEvent(new LeaveRequestApproved(Id, EmployeeId));

        return newStatus;
    }

    public Result<RequestStatus> Reject(
        EmployeeId actorId, string reason,
        EmployeeId? onBehalfOf, DateTimeOffset decidedAt)
    {
        if (reason.Length < 10)
            return DomainErrors.RejectionReasonTooShort;

        var activeStep = _steps.FirstOrDefault(s => s.State == StepState.Active);
        if (activeStep is null)
            return DomainErrors.NotAuthorizedToAct;

        var targetApprover = onBehalfOf ?? actorId;
        if (!activeStep.ApproverIds.Contains(targetApprover))
            return DomainErrors.NotAuthorizedToAct;

        activeStep.AddDecision(new ApprovalDecision(actorId, DecisionType.Rejected, decidedAt, reason, onBehalfOf));
        activeStep.State = StepState.Rejected;

        foreach (var step in _steps.Where(s => s.State == StepState.Waiting))
            step.State = StepState.Skipped;

        RaiseDomainEvent(new LeaveRequestRejected(Id, EmployeeId, reason));

        return DeriveStatus();
    }

    public Result<RequestStatus> Cancel(EmployeeId cancelledBy, string reason, DateOnly today)
    {
        var status = DeriveStatus();

        if (status == RequestStatus.Approved && StartDate <= today)
            return DomainErrors.CannotCancelStartedLeave;

        if (status != RequestStatus.PendingSupervisor &&
            status != RequestStatus.PendingManager &&
            status != RequestStatus.Approved)
            return DomainErrors.NotAuthorizedToAct;

        foreach (var step in _steps.Where(s => s.State is StepState.Active or StepState.Waiting))
            step.State = StepState.Skipped;

        RaiseDomainEvent(new LeaveRequestCancelled(Id, EmployeeId));

        return RequestStatus.Cancelled;
    }

    private RequestStatus DeriveStatus()
    {
        if (_steps.Any(s => s.State == StepState.Rejected))
            return RequestStatus.Rejected;

        if (_steps.All(s => s.State == StepState.Skipped) && _steps.Count > 0)
            return RequestStatus.Cancelled;

        var active = _steps.FirstOrDefault(s => s.State is not StepState.Done and not StepState.Skipped);
        if (active is null)
            return RequestStatus.Approved;

        return active.Kind == StepKind.Manager
            ? RequestStatus.PendingManager
            : RequestStatus.PendingSupervisor;
    }
}
