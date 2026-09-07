using MudBlazor;

namespace Assistec.Desktop.Theme;

public static class AssistecTheme
{
    public static MudTheme Default => new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#10233A",
            Background = "#F4F6F8",
            Surface = "#FFFFFF",
            AppbarBackground = "#FFFFFF",
            AppbarText = "#10233A",
            DrawerBackground = "#1B2A4A",
            DrawerText = "#FFFFFF",
            DrawerIcon = "#FFFFFF",
            TextPrimary = "#1B2733",
            TextSecondary = "#5A6B7A",
            Success = "#2E7D32"
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "12px"
        }
    };
}