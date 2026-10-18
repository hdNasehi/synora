using Microsoft.EntityFrameworkCore;
using Orange.Application.Abstractions;
using Orange.Application.Common;
using Orange.Domain.Common;
using Orange.Domain.Leave;

namespace Orange.Application.Approvals.GetInbox;

public sealed class GetInboxHandler
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetInboxHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<InboxItem>> HandleAsync(
        GetInboxQuery query, CancellationToken ct = default)
    {
        var actorId = _currentUser.EmployeeId;

        var requests = await _db.LeaveRequests
            .Include(r => r.Steps)
            .AsNoTracking()
            .ToListAsync(ct);

        // Filter to requests where this actor has an active step they can act on
        var pending = requests
            .Where(r =>
            {
                var active = r.Steps.FirstOrDefault(s => s.State == StepState.Active);
                return active is not null && active.ApproverIds.Contains(actorId);
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

        var items = page.Select(r =>
        {
            var active = r.Steps.First(s => s.State == StepState.Active);
            return new InboxItem(
                r.Id,
                r.EmployeeId,
                employees.TryGetValue(r.EmployeeId, out var name) ? name : "",
                r.Type, r.StartDate, r.EndDate, r.Amount, r.Unit, r.SubmittedAt,
                active.Order, active.Kind);
        }).ToList();

        return new PaginatedResult<InboxItem>(items, query.Page, query.PageSize, total);
    }
}
