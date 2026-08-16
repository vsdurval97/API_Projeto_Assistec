using System.Diagnostics;

namespace Assistec.Desktop.Services;

//Só funciona em desenvolvimento (localiza a solução subindo a árvore de diretórios) —
// numa instalação publicada isso não encontra nada e desiste em
// silêncio; a tela de Painel já trata "API indisponível" normalmente.
public static class ApiLauncher
{
    private const string UrlBase = "http://localhost:5170/";
    private static Process? _processoApi;

    public static async Task GarantirApiRodandoAsync()
    {
        if (await ApiEstaRespondendoAsync())
        {
            return; // já tem uma instância rodando (ex: você iniciou na mão)
        }

        var caminhoCsproj = EncontrarCsprojDaApi();
        if (caminhoCsproj is null)
        {
            return;
        }

        _processoApi = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{caminhoCsproj}\" --no-build",
                WorkingDirectory = Path.GetDirectoryName(caminhoCsproj),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        try
        {
            _processoApi.Start();
        }
        catch
        {
            return; // dotnet CLI indisponível ou outro problema — desiste em silêncio
        }

        for (var tentativa = 0; tentativa < 20; tentativa++)
        {
            if (await ApiEstaRespondendoAsync())
            {
                return;
            }
            await Task.Delay(500);
        }
    }

    private static async Task<bool> ApiEstaRespondendoAsync()
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            var resposta = await http.GetAsync(UrlBase + "api/Cliente");
            return resposta.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static string? EncontrarCsprojDaApi()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (dir.GetFiles("*.slnx").Length > 0 || dir.GetFiles("*.sln").Length > 0)
            {
                var candidato = Path.Combine(dir.FullName, "AssisTec", "AssisTec.csproj");
                return File.Exists(candidato) ? candidato : null;
            }
            dir = dir.Parent;
        }
        return null;
    }

    // Só mata o processo se foi o Desktop quem subiu — não interfere
    // numa instância que já estava rodando antes por conta própria.
    public static void Encerrar()
    {
        if (_processoApi is { HasExited: false })
        {
            try { _processoApi.Kill(entireProcessTree: true); }
            catch { /* já pode ter morrido sozinho */ }
        }
    }
}