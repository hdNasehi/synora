using MudBlazor;

namespace Orange.Web;

public static class OrangeTheme
{
    public static MudTheme Instance { get; } = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#1d4f91",
            PrimaryContrastText = "#ffffff",
            Secondary = "#c2410c",
            SecondaryContrastText = "#ffffff",
            Success = "#1d7a45",
            Warning = "#a35d00",
            Error = "#b42318",
            Background = "#f4f5f7",
            Surface = "#ffffff",
            TextPrimary = "#161b22",
            TextSecondary = "#566170",
            AppbarBackground = "#ffffff",
            AppbarText = "#161b22",
            DrawerBackground = "#ffffff",
            DrawerText = "#161b22",
            DrawerIcon = "#566170",
            Divider = "#e0e4ea",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#86b0ea",
            PrimaryContrastText = "#101317",
            Secondary = "#f2925a",
            Success = "#5cc68a",
            Warning = "#f0b44c",
            Error = "#f47c70",
            Background = "#101317",
            Surface = "#181c22",
            TextPrimary = "#e9ecf0",
            TextSecondary = "#a2acb8",
            AppbarBackground = "#181c22",
            AppbarText = "#e9ecf0",
            DrawerBackground = "#181c22",
            DrawerText = "#e9ecf0",
            DrawerIcon = "#a2acb8",
            Divider = "#2a313a",
        },
        Typography = new Typography
        {
            Default = new Default
            {
                FontFamily = ["IBM Plex Sans", "ui-sans-serif", "system-ui", "sans-serif"],
                FontSize = "0.875rem",
                LineHeight = 1.4286,
            },
            H5 = new H5 { FontSize = "1.25rem", FontWeight = 600 },
            H6 = new H6 { FontSize = "1rem", FontWeight = 600 },
            Subtitle1 = new Subtitle1 { FontSize = "0.875rem", FontWeight = 500 },
            Body1 = new Body1 { FontSize = "0.875rem", LineHeight = 1.4286 },
            Body2 = new Body2 { FontSize = "0.8125rem", LineHeight = 1.3846 },
            Caption = new Caption { FontSize = "0.75rem", FontWeight = 500 },
        },
        LayoutProperties = new LayoutProperties
        {
            DrawerWidthLeft = "240px",
            AppbarHeight = "56px",
        },
    };
}
