namespace Orange.Domain.Common;

public static class DomainErrors
{
    public static Error EndBeforeStart => new("EndBeforeStart", "End date is before the start date.");
    public static Error ExceedsMaxSpan => new("ExceedsMaxSpan", "Requests are limited to 62 calendar days.");
    public static Error NoWorkingDays => new("NoWorkingDays", "The selected dates contain no working days.");
    public static Error InsufficientBalance => new("InsufficientBalance", "Not enough available leave.");
    public static Error OverlapExists => new("OverlapExists", "Overlaps an existing request.");
    public static Error RejectionReasonTooShort => new("RejectionReasonTooShort", "Rejection reason must be at least 10 characters.");
    public static Error NotAuthorizedToAct => new("NotAuthorizedToAct", "Not authorized to act on this step.");
    public static Error CannotApproveOwnRequest => new("CannotApproveOwnRequest", "Cannot approve your own request.");
    public static Error CannotCancelStartedLeave => new("CannotCancelStartedLeave", "Cannot cancel leave that has already started.");
    public static Error ManagerIsSupervisor => new("ManagerIsSupervisor", "Final manager cannot also be a supervisor.");
    public static Error SickLeaveInFuture => new("SickLeaveInFuture", "Sick leave cannot start in the future.");
    public static Error MinimumNoticeViolation => new("MinimumNoticeViolation", "Minimum notice period not met.");
}
