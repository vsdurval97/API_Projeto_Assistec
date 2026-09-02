using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using MudBlazor.Services;
using Microsoft.Maui.LifecycleEvents;
using Assistec.Desktop.Windowing;
#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using WinRT.Interop;
#endif
namespace Assistec.Desktop;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                // Fontes customizadas entram aqui quando definirmos a
                // paleta/tipografia definitiva.
            });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddMudServices();
        builder.Services.AddSingleton<WindowManagerService>();
        builder.ConfigureLifecycleEvents(events =>
{
#if WINDOWS
    events.AddWindows(windows => windows.OnWindowCreated(window =>
    {
        var handle = WindowNative.GetWindowHandle(window);
        var id = Win32Interop.GetWindowIdFromWindow(handle);
        var appWindow = AppWindow.GetFromWindowId(id);
        appWindow.Resize(new Windows.Graphics.SizeInt32(1920, 1080));
    }));
#endif
});

        builder.Services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5170/"),
            Timeout = TimeSpan.FromSeconds(10)
        });

        builder.Services.AddSingleton(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        });

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif



        return builder.Build();
    }
}