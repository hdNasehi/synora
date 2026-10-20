using MudBlazor.Services;
using Serilog;
using Orange.Application;
using Orange.Application.Abstractions;
using Orange.Infrastructure;
using Orange.Infrastructure.Persistence.Seed;
using Orange.Web.Auth;
using Orange.Web.Components;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.Services.AddMudServices();

    builder.Services.AddSingleton(TimeProvider.System);

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Server=(localdb)\\mssqllocaldb;Database=Orange;Trusted_Connection=True";
    builder.Services.AddInfrastructure(connectionString);
    builder.Services.AddApplication();

    // Development auth: persona switcher
    builder.Services.AddScoped<PersonaHolder>();
    builder.Services.AddScoped<ICurrentUser>(sp =>
        sp.GetRequiredService<PersonaHolder>().Get());

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseMiddleware<PersonaMiddleware>();

    // Development persona switcher — sets cookie then redirects
    app.MapGet("/switch-persona/{key}", (string key, HttpResponse response) =>
    {
        if (PersonaSwitcherCurrentUser.Available.ContainsKey(key))
            response.Cookies.Append("orange-persona", key,
                new CookieOptions { Path = "/", SameSite = SameSiteMode.Lax });
        return Results.Redirect("/");
    });

    app.UseAntiforgery();

    app.MapStaticAssets();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    if (app.Environment.IsDevelopment())
        await DevelopmentSeeder.SeedAsync(app);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
