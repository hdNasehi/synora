using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Orange.Application.Approvals.Approve;
using Orange.Application.Approvals.GetFinalApprovals;
using Orange.Application.Approvals.GetInbox;
using Orange.Application.Approvals.Reject;
using Orange.Application.Calendar;
using Orange.Application.Requests.Cancel;
using Orange.Application.Requests.GetDetails;
using Orange.Application.Requests.GetMyRequests;
using Orange.Application.Requests.Preview;
using Orange.Application.Requests.Submit;
using Orange.Domain.Services;

namespace Orange.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<SubmitLeaveRequestValidator>(
            ServiceLifetime.Scoped, includeInternalTypes: true);

        services.AddScoped<SubmitLeaveRequestHandler>();
        services.AddScoped<CancelLeaveRequestHandler>();
        services.AddScoped<PreviewCalculationHandler>();
        services.AddScoped<GetMyRequestsHandler>();
        services.AddScoped<GetRequestDetailsHandler>();
        services.AddScoped<ApproveRequestHandler>();
        services.AddScoped<RejectRequestHandler>();
        services.AddScoped<GetInboxHandler>();
        services.AddScoped<GetFinalApprovalsHandler>();
        services.AddScoped<GetTeamCalendarHandler>();

        return services;
    }
}
