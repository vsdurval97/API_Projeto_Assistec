using AssistenciaTecnica.Api.Data;
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
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open(); // precisa ficar aberta; SQLite ":memory:" some ao fechar a última conexão

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

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