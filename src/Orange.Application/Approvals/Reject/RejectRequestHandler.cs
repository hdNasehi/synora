using Microsoft.EntityFrameworkCore;
using Orange.Application.Abstractions;
using Orange.Domain.Common;
using Orange.Domain.Leave;

namespace Orange.Application.Approvals.Reject;

public sealed class RejectRequestHandler
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditLog _audit;
    private readonly IOutbox _outbox;
    private readonly TimeProvider _time;

    public RejectRequestHandler(
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
        RejectRequestCommand command, CancellationToken ct = default)
    {
        var request = await _db.LeaveRequests
            .Include(r => r.Steps)
            .FirstOrDefaultAsync(r => r.Id == command.RequestId, ct);

        if (request is null)
            return new Error("NotFound", "Leave request not found.");

        var result = request.Reject(
            _currentUser.EmployeeId, command.Reason,
            command.OnBehalfOf, _time.GetUtcNow());

        if (!result.IsSuccess) return result;

        foreach (var evt in request.DomainEvents)
            _outbox.Enqueue(evt);
        request.ClearDomainEvents();

        _audit.Record("Rejected leave request",
            new { RequestId = command.RequestId, command.Reason },
            _currentUser.EmployeeId);

        await _db.SaveChangesAsync(ct);
        return result;
    }
}
