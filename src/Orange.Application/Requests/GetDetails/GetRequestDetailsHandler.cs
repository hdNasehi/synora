using Microsoft.EntityFrameworkCore;
using Orange.Application.Abstractions;
using Orange.Domain.Common;

namespace Orange.Application.Requests.GetDetails;

public sealed class GetRequestDetailsHandler
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetRequestDetailsHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<LeaveRequestDetail>> HandleAsync(
        GetRequestDetailsQuery query, CancellationToken ct = default)
    {
        var request = await _db.LeaveRequests
            .Include(r => r.Steps)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == query.RequestId, ct);

        if (request is null)
            return new Error("NotFound", "Leave request not found.");

        var empId = _currentUser.EmployeeId;
        var canView = request.EmployeeId == empId ||
                      _currentUser.IsInRole(AppRole.HrAdmin) ||
                      _currentUser.IsInRole(AppRole.Manager) ||
                      _currentUser.IsInRole(AppRole.Supervisor);

        if (!canView)
            return new Error("Forbidden", "Not authorized to view this request.");

        var steps = request.Steps
            .OrderBy(s => s.Order)
            .Select(s => new ApprovalStepDetail(
                s.Id, s.Order, s.Kind, s.Rule, s.State, s.ApproverIds,
                s.Decisions.Select(d => new ApprovalDecisionDetail(
                    d.DecidedBy, d.Decision, d.DecidedAt, d.Comment, d.OnBehalfOf)).ToList()))
            .ToList();

        return new LeaveRequestDetail(
            request.Id, request.Type, request.StartDate, request.EndDate,
            request.PartialTime, request.Amount, request.Unit, request.Reason,
            request.Status, request.Calculation, steps, request.SubmittedAt);
    }
}
