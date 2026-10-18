using Orange.Domain.Common;
using Orange.Domain.Employees;
using Orange.Domain.Leave;

namespace Orange.Domain.Services;

public sealed class ApprovalRouteBuilder : IApprovalRouteBuilder
{
    public Result<IReadOnlyList<ApprovalStep>> Build(
        EmployeeAssignment assignment,
        LeaveType type)
    {
        if (type == LeaveType.Sick)
        {
            var notifyStep = new ApprovalStep
            {
                Order = 1,
                Kind = StepKind.Notify,
                Rule = ApprovalRule.None,
                ApproverIds = assignment.SupervisorIds,
                State = StepState.Done
            };
            return new List<ApprovalStep> { notifyStep };
        }

        var steps = new List<ApprovalStep>();
        var supervisors = assignment.SupervisorIds;

        if (supervisors.Count > 0 && assignment.Rule != ApprovalRule.None)
        {
            if (supervisors.Contains(assignment.ManagerId))
                return DomainErrors.ManagerIsSupervisor;

            if (assignment.Rule == ApprovalRule.Sequential)
            {
                for (var i = 0; i < supervisors.Count; i++)
                {
                    steps.Add(new ApprovalStep
                    {
                        Order = i + 1,
                        Kind = StepKind.Supervisor,
                        Rule = ApprovalRule.Sequential,
                        ApproverIds = [supervisors[i]],
                        State = i == 0 ? StepState.Active : StepState.Waiting
                    });
                }
            }
            else
            {
                steps.Add(new ApprovalStep
                {
                    Order = 1,
                    Kind = StepKind.Supervisor,
                    Rule = assignment.Rule,
                    ApproverIds = supervisors,
                    State = StepState.Active
                });
            }
        }

        steps.Add(new ApprovalStep
        {
            Order = steps.Count + 1,
            Kind = StepKind.Manager,
            Rule = ApprovalRule.None,
            ApproverIds = [assignment.ManagerId],
            State = steps.Count > 0 ? StepState.Waiting : StepState.Active
        });

        return steps;
    }
}
