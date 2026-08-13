// AssisTec.Tests/CustomWebApplicationFactory.cs
using AssisTec.Tests.Fakes;
using AssistenciaTecnica.Api.Data;
using AssistenciaTecnica.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AssisTec.Tests;

// Sobe a API inteira via TestServer, com o pipeline HTTP real (ModelState,
// desserialização JSON, [JsonRequired], etc.) — diferente dos testes de
// controller, que chamam o método diretamente e pulam esse pipeline.
//
// Usa o provider SQLite de verdade (não Microsoft.EntityFrameworkCore.
// InMemory), porque comportamentos que já causaram bug real neste projeto
// — perda de DateTimeKind no round-trip, enforcement de concorrência
// otimista, busy_timeout — só se manifestam com o provider real. Uma
// conexão SQLite em memória (":memory:") mantida aberta durante a vida da
// factory simula o arquivo .db de produção sem precisar de disco.
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection _connection = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (dbDescriptor is not null)
            {
                services.Remove(dbDescriptor);
            }

            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open(); // precisa ficar aberta; SQLite ":memory:" some ao fechar a última conexão

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

            // Remove TODOS os descriptors relacionados a ICepLocalizadorService,
            // incluindo os registrados internamente por AddHttpClient (que cria
            // mais de um ServiceDescriptor por trás do typed client) — remover
            // só um pode não ser suficiente para garantir que o fake seja
            // resolvido em vez do HttpClient real batendo no ViaCEP de verdade.
            var cepDescriptors = services
                .Where(d => d.ServiceType == typeof(ICepLocalizadorService))
                .ToList();
            foreach (var descriptor in cepDescriptors)
            {
                services.Remove(descriptor);
            }
            services.AddScoped<ICepLocalizadorService, FakeCepLocalizadorService>();

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Migrate() em vez de EnsureCreated(): aplica o MESMO schema que
            // roda em produção (incluindo UltimaModificacaoUtc como
            // concurrency token), não uma versão simplificada do modelo.
            db.Database.Migrate();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Dispose();
    }
}