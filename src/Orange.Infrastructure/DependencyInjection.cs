using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Orange.Application.Abstractions;
using Orange.Domain.Services;
using Orange.Infrastructure.Audit;
using Orange.Infrastructure.Outbox;
using Orange.Infrastructure.Persistence;

namespace Orange.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<OrangeDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<OrangeDbContext>());
        services.AddScoped<IAuditLog, AuditLogService>();
        services.AddScoped<IOutbox, OutboxService>();

        services.AddScoped<IWorkingDayCalculator, WorkingDayCalculator>();
        services.AddScoped<IApprovalRouteBuilder, ApprovalRouteBuilder>();
        services.AddScoped<IBalanceCalculator, BalanceCalculator>();
        services.AddScoped<IAvailabilityChecker, AvailabilityChecker>();

        return services;
    }
}
