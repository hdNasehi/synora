using Microsoft.EntityFrameworkCore;
using Orange.Application.Abstractions;
using Orange.Domain.Common;
using Orange.Domain.Leave;

namespace Orange.Application.Requests.Cancel;

public sealed class CancelLeaveRequestHandler
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditLog _audit;
    private readonly IOutbox _outbox;
    private readonly TimeProvider _time;

    public CancelLeaveRequestHandler(
        IAppDbContext db, ICurrentUser currentUser,
        IAuditLog audit, IOutbox outbox, TimeProvider time)
    {
        _db = db;
        _currentUser = currentUser;
        _audit = audit;
        _outbox = outbox;
        _time = time;
    }

    public async Task<Result<RequestStatus>> HandleAsync(
        CancelLeaveRequest command, CancellationToken ct = default)
    {
        var request = await _db.LeaveRequests
            .Include(r => r.Steps)
            .FirstOrDefaultAsync(r => r.Id == command.RequestId, ct);

        if (request is null)
            return new Error("NotFound", "Leave request not found.");

        var empId = _currentUser.EmployeeId;
        if (request.EmployeeId != empId && !_currentUser.IsInRole(AppRole.HrAdmin))
            return new Error("Forbidden", "Not authorized.");

        var today = DateOnly.FromDateTime(_time.GetUtcNow().DateTime);
        var result = request.Cancel(empId, command.Reason, today);
        if (!result.IsSuccess) return result;

        foreach (var evt in request.DomainEvents)
            _outbox.Enqueue(evt);
        request.ClearDomainEvents();

        _audit.Record("Cancelled leave request", new { RequestId = command.RequestId }, empId);
        await _db.SaveChangesAsync(ct);

        return result;
    }
}
