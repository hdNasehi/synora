using FluentAssertions;
using Orange.Domain.Balances;
using Orange.Domain.Common;
using Orange.Domain.Employees;
using Orange.Domain.Leave;
using Orange.Domain.Services;

namespace Orange.Domain.Tests;

public class BalanceCalculatorTests
{
    private static readonly EmployeeId SanneId = new(Guid.NewGuid());
    private static readonly EmployeeId EvaId = new(Guid.NewGuid());
    private static readonly LeavePolicyId StdPolicyId = new(Guid.NewGuid());
    private static readonly DateOnly Today = new(2026, 10, 19);

    private readonly BalanceCalculator _sut = new();

    private static Employee MakeSanne()
    {
        var emp = new Employee { FirstName = "Sanne", LastName = "de Vries", Fte = 1m };
        emp.SetIdForTest(SanneId);
        return emp;
    }

    private static LeavePolicy MakeStdPolicy()
    {
        var pol = new LeavePolicy { Name = "Standard", Unit = BalanceUnit.Days, Entitlement = 25 };
        pol.SetIdForTest(StdPolicyId);
        return pol;
    }

    private static LeaveRequest MakeApprovedRequest(decimal amount, DateOnly start, DateOnly end)
    {
        var steps = new List<ApprovalStep>
        {
            new() { Order = 1, Kind = StepKind.Manager, Rule = ApprovalRule.None, ApproverIds = [EvaId], State = StepState.Done }
        };
        return LeaveRequest.Submit(
            SanneId, LeaveType.Annual, start, end, null,
            amount, BalanceUnit.Days, null,
            new LeaveCalculation([], amount, amount * 8, [], [], []),
            steps, new DateTimeOffset(2026, 1, 15, 9, 0, 0, TimeSpan.Zero));
    }

    private static LeaveRequest MakePendingRequest(decimal amount, DateOnly start, DateOnly end)
    {
        var steps = new List<ApprovalStep>
        {
            new() { Order = 1, Kind = StepKind.Supervisor, Rule = ApprovalRule.Any, ApproverIds = [EvaId], State = StepState.Active },
            new() { Order = 2, Kind = StepKind.Manager, Rule = ApprovalRule.None, ApproverIds = [EvaId], State = StepState.Waiting }
        };
        return LeaveRequest.Submit(
            SanneId, LeaveType.Annual, start, end, null,
            amount, BalanceUnit.Days, null,
            new LeaveCalculation([], amount, amount * 8, [], [], []),
            steps, new DateTimeOffset(2026, 10, 10, 9, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void Sanne_balance_check_initial()
    {
        // Sanne 2026: entitlement 25, carry 3, adjustment +1, approved 17.5, pending 2 → available 9.5
        var requests = new List<LeaveRequest>
        {
            MakeApprovedRequest(10, new DateOnly(2026, 3, 2), new DateOnly(2026, 3, 13)),
            MakeApprovedRequest(5, new DateOnly(2026, 7, 6), new DateOnly(2026, 7, 10)),
            MakeApprovedRequest(2.5m, new DateOnly(2026, 8, 17), new DateOnly(2026, 8, 19)),
            MakePendingRequest(2, new DateOnly(2026, 12, 28), new DateOnly(2026, 12, 29)),
        };

        var adjustments = new List<BalanceAdjustment>
        {
            new() { Id = Guid.NewGuid(), EmployeeId = SanneId, Kind = AdjustmentKind.CarryOver, Amount = 3, Year = 2026 },
            new() { Id = Guid.NewGuid(), EmployeeId = SanneId, Kind = AdjustmentKind.Adjustment, Amount = 1, Year = 2026 },
        };

        var balance = _sut.Calculate(MakeSanne(), MakeStdPolicy(), requests, adjustments, 2026, Today);

        balance.Entitlement.Should().Be(25);
        balance.CarryOver.Should().Be(3);
        balance.Adjustments.Should().Be(1);
        balance.Approved.Should().Be(17.5m);
        balance.Pending.Should().Be(2);
        balance.Available.Should().Be(9.5m);
    }

    [Fact]
    public void Sanne_balance_after_submitting_3_day_request()
    {
        // After submitting 12–16 Nov (3 days): pending 5, available 6.5
        var requests = new List<LeaveRequest>
        {
            MakeApprovedRequest(10, new DateOnly(2026, 3, 2), new DateOnly(2026, 3, 13)),
            MakeApprovedRequest(5, new DateOnly(2026, 7, 6), new DateOnly(2026, 7, 10)),
            MakeApprovedRequest(2.5m, new DateOnly(2026, 8, 17), new DateOnly(2026, 8, 19)),
            MakePendingRequest(2, new DateOnly(2026, 12, 28), new DateOnly(2026, 12, 29)),
            MakePendingRequest(3, new DateOnly(2026, 11, 12), new DateOnly(2026, 11, 16)),
        };

        var adjustments = new List<BalanceAdjustment>
        {
            new() { Id = Guid.NewGuid(), EmployeeId = SanneId, Kind = AdjustmentKind.CarryOver, Amount = 3, Year = 2026 },
            new() { Id = Guid.NewGuid(), EmployeeId = SanneId, Kind = AdjustmentKind.Adjustment, Amount = 1, Year = 2026 },
        };

        var balance = _sut.Calculate(MakeSanne(), MakeStdPolicy(), requests, adjustments, 2026, Today);

        balance.Pending.Should().Be(5);
        balance.Available.Should().Be(6.5m);
    }

    [Fact]
    public void Sick_leave_does_not_affect_annual_balance()
    {
        var sickRequest = LeaveRequest.Submit(
            SanneId, LeaveType.Sick,
            new DateOnly(2026, 10, 15), new DateOnly(2026, 10, 16), null,
            2, BalanceUnit.Days, null,
            new LeaveCalculation([], 2, 16, [], [], []),
            [new ApprovalStep { Order = 1, Kind = StepKind.Notify, Rule = ApprovalRule.None, ApproverIds = [], State = StepState.Done }],
            new DateTimeOffset(2026, 10, 15, 8, 0, 0, TimeSpan.Zero));

        var balance = _sut.Calculate(MakeSanne(), MakeStdPolicy(), [sickRequest], [], 2026, Today);

        balance.Approved.Should().Be(0);
        balance.Available.Should().Be(25);
    }
}
