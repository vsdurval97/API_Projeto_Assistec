using Microsoft.Extensions.Logging;

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

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // Registro do HttpClient pra API local vem na Parte 2.

        return builder.Build();
    }
}