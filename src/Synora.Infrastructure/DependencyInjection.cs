using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Synora.Application.Abstractions;
using Synora.Infrastructure.Persistence;

namespace Synora.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<SynoraDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<SynoraDbContext>());

        return services;
    }
}
