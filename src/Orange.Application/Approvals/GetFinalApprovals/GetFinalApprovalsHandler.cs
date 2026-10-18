using Microsoft.EntityFrameworkCore;
using Orange.Application.Abstractions;
using Orange.Application.Common;
using Orange.Domain.Common;
using Orange.Domain.Leave;

namespace Orange.Application.Approvals.GetFinalApprovals;

public sealed class GetFinalApprovalsHandler
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetFinalApprovalsHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<FinalApprovalItem>> HandleAsync(
        GetFinalApprovalsQuery query, CancellationToken ct = default)
    {
        var actorId = _currentUser.EmployeeId;

        var requests = await _db.LeaveRequests
            .Include(r => r.Steps)
            .AsNoTracking()
            .ToListAsync(ct);

        var pending = requests
            .Where(r =>
            {
                var active = r.Steps.FirstOrDefault(s => s.State == StepState.Active);
                return active is { Kind: StepKind.Manager } && active.ApproverIds.Contains(actorId);
            })
            .OrderBy(r => r.SubmittedAt)
            .ToList();

        var total = pending.Count;
        var page = pending
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var employeeIds = page.Select(r => r.EmployeeId).Distinct().ToList();
        var employees = await _db.Employees
            .Where(e => employeeIds.Contains(e.Id))
            .AsNoTracking()
            .ToDictionaryAsync(e => e.Id, e => e.FullName, ct);

        var items = page.Select(r => new FinalApprovalItem(
            r.Id, r.EmployeeId,
            employees.TryGetValue(r.EmployeeId, out var name) ? name : "",
            r.Type, r.StartDate, r.EndDate, r.Amount, r.Unit, r.SubmittedAt))
            .ToList();

        return new PaginatedResult<FinalApprovalItem>(items, query.Page, query.PageSize, total);
    }
}
