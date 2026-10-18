using Orange.Application.Abstractions;

namespace Orange.Web.Auth;

/// <summary>
/// Reads the persona cookie and registers ICurrentUser for the request.
/// Development only — replaced by Entra ID middleware in production.
/// </summary>
public sealed class PersonaMiddleware
{
    private const string CookieName = "orange-persona";
    private readonly RequestDelegate _next;

    public PersonaMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var key = context.Request.Cookies[CookieName] ?? "e01";

        if (!PersonaSwitcherCurrentUser.Available.TryGetValue(key, out var persona))
            persona = PersonaSwitcherCurrentUser.Available["e01"];

        context.RequestServices
            .GetRequiredService<PersonaHolder>()
            .Set(new PersonaSwitcherCurrentUser(persona));

        await _next(context);
    }
}

/// <summary>
/// Scoped holder so middleware can inject ICurrentUser into DI via a factory.
/// </summary>
public sealed class PersonaHolder
{
    private ICurrentUser? _user;

    public void Set(ICurrentUser user) => _user = user;

    public ICurrentUser Get() =>
        _user ?? throw new InvalidOperationException("Persona not set for this request.");
}
