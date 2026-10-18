using Microsoft.EntityFrameworkCore;
using Orange.Application.Abstractions;
using Orange.Application.Common;
using Orange.Domain.Common;

namespace Orange.Application.Requests.GetMyRequests;

public sealed class GetMyRequestsHandler
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetMyRequestsHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PaginatedResult<LeaveRequestSummary>> HandleAsync(
        GetMyRequestsQuery query, CancellationToken ct = default)
    {
        var empId = _currentUser.EmployeeId;

        var q = _db.LeaveRequests
            .Include(r => r.Steps)
            .Where(r => r.EmployeeId == empId)
            .AsQueryable();

        if (query.Year.HasValue)
            q = q.Where(r => r.StartDate.Year == query.Year.Value);

        var all = await q.OrderByDescending(r => r.SubmittedAt).ToListAsync(ct);

        var filtered = query.Status.HasValue
            ? all.Where(r => r.Status == query.Status.Value).ToList()
            : all;

        var total = filtered.Count;
        var items = filtered
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(r => new LeaveRequestSummary(
                r.Id, r.Type, r.StartDate, r.EndDate,
                r.Amount, r.Unit, r.Status, r.SubmittedAt))
            .ToList();

        return new PaginatedResult<LeaveRequestSummary>(items, query.Page, query.PageSize, total);
    }
}
