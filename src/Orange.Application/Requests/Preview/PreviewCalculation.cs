using Orange.Domain.Leave;

namespace Orange.Application.Requests.Preview;

public sealed record PreviewCalculationRequest(
    DateOnly StartDate,
    DateOnly EndDate,
    TimeOnly? PartialFrom,
    TimeOnly? PartialTo);

public sealed record PreviewCalculationResponse(
    LeaveCalculation Calculation,
    decimal WorkingDays,
    decimal TotalHours);
