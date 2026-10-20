using MudBlazor;

namespace Orange.Web.Components.Shared;

internal sealed record DrawerAction(string Label, Color Color, Variant Variant, Func<Task> Execute);
