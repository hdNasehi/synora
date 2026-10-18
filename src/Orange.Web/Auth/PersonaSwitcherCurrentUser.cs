using Orange.Application.Abstractions;
using Orange.Domain.Common;
using Orange.Infrastructure.Persistence.Seed;

namespace Orange.Web.Auth;

/// <summary>
/// Development-only ICurrentUser that reads persona from a browser cookie.
/// Switch persona via the PersonaSwitcher component.
/// </summary>
public sealed class PersonaSwitcherCurrentUser : ICurrentUser
{
    private static readonly IReadOnlyDictionary<string, PersonaInfo> Personas =
        new Dictionary<string, PersonaInfo>
        {
            ["e01"] = new("e01", "Sanne de Vries", new HashSet<AppRole> { AppRole.Employee }),
            ["e02"] = new("e02", "Daan Bakker", new HashSet<AppRole> { AppRole.Employee, AppRole.Supervisor }),
            ["e13"] = new("e13", "Eva Jansen", new HashSet<AppRole> { AppRole.Employee, AppRole.Manager }),
            ["e14"] = new("e14", "Lotte Visser", new HashSet<AppRole> { AppRole.Employee, AppRole.HrAdmin }),
        };

    public static IReadOnlyDictionary<string, PersonaInfo> Available => Personas;

    private readonly PersonaInfo _persona;

    public PersonaSwitcherCurrentUser(PersonaInfo persona)
    {
        _persona = persona;
    }

    public EmployeeId EmployeeId =>
        new(DevelopmentSeeder.SeedIds[_persona.Key]);

    public IReadOnlySet<AppRole> Roles => _persona.Roles;

    public bool IsInRole(AppRole role) => _persona.Roles.Contains(role);
}

public sealed record PersonaInfo(
    string Key,
    string Name,
    IReadOnlySet<AppRole> Roles);
