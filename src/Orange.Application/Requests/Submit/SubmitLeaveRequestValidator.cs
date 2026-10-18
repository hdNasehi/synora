using FluentValidation;
using Orange.Domain.Common;

namespace Orange.Application.Requests.Submit;

public sealed class SubmitLeaveRequestValidator : AbstractValidator<SubmitLeaveRequest>
{
    public SubmitLeaveRequestValidator()
    {
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty()
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be on or after start date.");

        RuleFor(x => x.Reason).MaximumLength(500);

        When(x => x.PartialFrom.HasValue || x.PartialTo.HasValue, () =>
        {
            RuleFor(x => x.PartialFrom).NotNull().WithMessage("Partial from time is required.");
            RuleFor(x => x.PartialTo).NotNull().WithMessage("Partial to time is required.");
            RuleFor(x => x.StartDate).Equal(x => x.EndDate)
                .WithMessage("Partial days must be a single date.");
        });
    }
}
