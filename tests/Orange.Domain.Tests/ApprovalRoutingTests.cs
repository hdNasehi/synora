using FluentAssertions;
using Orange.Domain.Common;
using Orange.Domain.Employees;
using Orange.Domain.Leave;
using Orange.Domain.Services;

namespace Orange.Domain.Tests;

public class ApprovalRoutingTests
{
    private static readonly EmployeeId Sanne = new(Guid.NewGuid());
    private static readonly EmployeeId Joris = new(Guid.NewGuid());
    private static readonly EmployeeId Ahmed = new(Guid.NewGuid());
    private static readonly EmployeeId Daan = new(Guid.NewGuid());
    private static readonly EmployeeId Priya = new(Guid.NewGuid());
    private static readonly EmployeeId Bram = new(Guid.NewGuid());
    private static readonly EmployeeId Eva = new(Guid.NewGuid());

    private readonly ApprovalRouteBuilder _routeBuilder = new();
    private readonly DateTimeOffset _now = new(2026, 10, 19, 10, 0, 0, TimeSpan.Zero);

    private static EmployeeAssignment MakeAssignment(
        EmployeeId empId, IReadOnlyList<EmployeeId> supervisors,
        ApprovalRule rule, EmployeeId managerId) =>
        new()
        {
            Id = Guid.NewGuid(),
            EmployeeId = empId,
            SupervisorIds = supervisors,
            Rule = rule,
            ManagerId = managerId,
            ValidFrom = new DateOnly(2025, 4, 1)
        };

    private static LeaveRequest SubmitRequest(
        EmployeeId empId, IReadOnlyList<ApprovalStep> steps, decimal amount = 3) =>
        LeaveRequest.Submit(
            empId, LeaveType.Annual,
            new DateOnly(2026, 11, 12), new DateOnly(2026, 11, 16),
            null, amount, BalanceUnit.Days, null,
            new LeaveCalculation([], amount, amount * 8, [], [], []),
            steps,
            new DateTimeOffset(2026, 10, 19, 10, 0, 0, TimeSpan.Zero));

    [Fact]
    public void Sanne_all_rule_requires_both_supervisors_then_manager()
    {
        // Sanne (all: Daan, Priya): Daan approves → still Pending Supervisor;
        // Priya approves → Pending Manager; Eva approves → Approved.
        var assignment = MakeAssignment(Sanne, [Daan, Priya], ApprovalRule.All, Eva);
        var stepsResult = _routeBuilder.Build(assignment, LeaveType.Annual);
        stepsResult.IsSuccess.Should().BeTrue();

        var request = SubmitRequest(Sanne, stepsResult.Value!);
        request.Status.Should().Be(RequestStatus.PendingSupervisor);

        var r1 = request.Approve(Daan, "ok", null, _now);
        r1.IsSuccess.Should().BeTrue();
        r1.Value!.Should().Be(RequestStatus.PendingSupervisor);

        var r2 = request.Approve(Priya, "ok", null, _now);
        r2.IsSuccess.Should().BeTrue();
        r2.Value!.Should().Be(RequestStatus.PendingManager);

        var r3 = request.Approve(Eva, "ok", null, _now);
        r3.IsSuccess.Should().BeTrue();
        r3.Value!.Should().Be(RequestStatus.Approved);
    }

    [Fact]
    public void Joris_any_rule_single_supervisor_sufficient()
    {
        // Joris (any: Daan, Priya): Daan approves → Pending Manager.
        var assignment = MakeAssignment(Joris, [Daan, Priya], ApprovalRule.Any, Eva);
        var steps = _routeBuilder.Build(assignment, LeaveType.Annual).Value!;
        var request = SubmitRequest(Joris, steps);

        var r = request.Approve(Daan, "ok", null, _now);
        r.Value!.Should().Be(RequestStatus.PendingManager);
    }

    [Fact]
    public void Ahmed_sequential_rule_enforces_order()
    {
        // Ahmed (sequential: Bram, Priya): Priya can't act until Bram approved.
        var assignment = MakeAssignment(Ahmed, [Bram, Priya], ApprovalRule.Sequential, Eva);
        var steps = _routeBuilder.Build(assignment, LeaveType.Annual).Value!;
        var request = SubmitRequest(Ahmed, steps);

        steps.Should().HaveCount(3);
        steps[0].ApproverIds.Should().Contain(Bram);
        steps[0].State.Should().Be(StepState.Active);
        steps[1].ApproverIds.Should().Contain(Priya);
        steps[1].State.Should().Be(StepState.Waiting);

        var r1 = request.Approve(Priya, null, null, _now);
        r1.IsSuccess.Should().BeFalse();
        r1.Error!.Code.Should().Be("NotAuthorizedToAct");

        var r2 = request.Approve(Bram, null, null, _now);
        r2.Value!.Should().Be(RequestStatus.PendingSupervisor);

        var r3 = request.Approve(Priya, null, null, _now);
        r3.Value!.Should().Be(RequestStatus.PendingManager);
    }

    [Fact]
    public void Substitute_can_act_on_behalf_of_absent_approver()
    {
        // Daan absent, substitute Priya: Priya acts on behalf of Daan.
        var assignment = MakeAssignment(Sanne, [Daan, Priya], ApprovalRule.All, Eva);
        var steps = _routeBuilder.Build(assignment, LeaveType.Annual).Value!;
        var request = SubmitRequest(Sanne, steps);

        // Priya acts on behalf of Daan
        var r1 = request.Approve(Priya, "on behalf", Daan, _now);
        r1.IsSuccess.Should().BeTrue();

        // Priya also approves for herself
        var r2 = request.Approve(Priya, "ok", null, _now);
        r2.Value!.Should().Be(RequestStatus.PendingManager);
    }

    [Fact]
    public void Reject_without_reason_returns_error()
    {
        var assignment = MakeAssignment(Sanne, [Daan], ApprovalRule.Any, Eva);
        var steps = _routeBuilder.Build(assignment, LeaveType.Annual).Value!;
        var request = SubmitRequest(Sanne, steps);

        var r = request.Reject(Daan, "too short", null, _now);
        r.IsSuccess.Should().BeFalse();
        r.Error!.Code.Should().Be("RejectionReasonTooShort");
    }

    [Fact]
    public void Reject_with_reason_skips_remaining_steps()
    {
        var assignment = MakeAssignment(Sanne, [Daan], ApprovalRule.Any, Eva);
        var steps = _routeBuilder.Build(assignment, LeaveType.Annual).Value!;
        var request = SubmitRequest(Sanne, steps);

        var r = request.Reject(Daan, "Not enough coverage during this period", null, _now);
        r.Value!.Should().Be(RequestStatus.Rejected);
        request.Steps.Last().State.Should().Be(StepState.Skipped);
    }

    [Fact]
    public void Manager_listed_as_supervisor_returns_error()
    {
        var assignment = MakeAssignment(Sanne, [Eva, Daan], ApprovalRule.All, Eva);
        var result = _routeBuilder.Build(assignment, LeaveType.Annual);
        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("ManagerIsSupervisor");
    }

    [Fact]
    public void Sick_leave_creates_single_notify_step()
    {
        var assignment = MakeAssignment(Sanne, [Daan, Priya], ApprovalRule.All, Eva);
        var steps = _routeBuilder.Build(assignment, LeaveType.Sick).Value!;

        steps.Should().HaveCount(1);
        steps[0].Kind.Should().Be(StepKind.Notify);
        steps[0].State.Should().Be(StepState.Done);
    }

    [Fact]
    public void Cannot_approve_own_request()
    {
        var assignment = MakeAssignment(Daan, [Priya], ApprovalRule.Any, Eva);
        var steps = _routeBuilder.Build(assignment, LeaveType.Annual).Value!;
        var request = SubmitRequest(Daan, steps);

        var r = request.Approve(Daan, null, null, _now);
        r.IsSuccess.Should().BeFalse();
        r.Error!.Code.Should().Be("CannotApproveOwnRequest");
    }

    [Fact]
    public void Cancel_approved_future_request_succeeds()
    {
        var assignment = MakeAssignment(Joris, [Daan], ApprovalRule.Any, Eva);
        var steps = _routeBuilder.Build(assignment, LeaveType.Annual).Value!;
        var request = SubmitRequest(Joris, steps);

        request.Approve(Daan, null, null, _now);
        request.Approve(Eva, null, null, _now);
        request.Status.Should().Be(RequestStatus.Approved);

        var r = request.Cancel(Joris, "changed plans", new DateOnly(2026, 10, 19));
        r.Value!.Should().Be(RequestStatus.Cancelled);
    }

    [Fact]
    public void Cancel_started_approved_request_fails()
    {
        var assignment = MakeAssignment(Joris, [Daan], ApprovalRule.Any, Eva);
        var steps = _routeBuilder.Build(assignment, LeaveType.Annual).Value!;
        var request = SubmitRequest(Joris, steps);

        request.Approve(Daan, null, null, _now);
        request.Approve(Eva, null, null, _now);

        var r = request.Cancel(Joris, "too late", new DateOnly(2026, 11, 15));
        r.IsSuccess.Should().BeFalse();
        r.Error!.Code.Should().Be("CannotCancelStartedLeave");
    }
}
