using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Orange.Application.Abstractions;
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

        return services;
    }
}
