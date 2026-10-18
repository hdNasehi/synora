using FluentValidation;

namespace Orange.Application.Approvals.Reject;

public sealed class RejectRequestValidator : AbstractValidator<RejectRequestCommand>
{
    public RejectRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty()
            .MinimumLength(10)
            .WithMessage("Rejection reason must be at least 10 characters.");
    }
}
