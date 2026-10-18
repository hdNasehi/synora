using Synora.Domain.Common;

namespace Synora.Domain.Leave;

public sealed class ApprovalStep : Entity<ApprovalStepId>
{
    private readonly List<ApprovalDecision> _decisions = [];

    public int Order { get; init; }
    public StepKind Kind { get; init; }
    public ApprovalRule Rule { get; init; }
    public IReadOnlyList<EmployeeId> ApproverIds { get; init; } = [];
    public StepState State { get; internal set; }
    public IReadOnlyList<ApprovalDecision> Decisions => _decisions.AsReadOnly();

    internal void AddDecision(ApprovalDecision decision) => _decisions.Add(decision);
}

public sealed record ApprovalDecision(
    EmployeeId DecidedBy,
    DecisionType Decision,
    DateTimeOffset DecidedAt,
    string? Comment,
    EmployeeId? OnBehalfOf);
