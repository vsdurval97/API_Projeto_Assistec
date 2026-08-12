using AssistenciaTecnica.Api.Data;
using AssistenciaTecnica.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configuração de Serviços

// DbContext - SQLite local
var connectionStringBase = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=assistencia.db";

var connectionString = new SqliteConnectionStringBuilder(connectionStringBase)
{
    DefaultTimeout = 5 // segundos de espera antes de lançar "database is locked"
}.ToString();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));
// Controladores
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Registrado como Scoped (não Singleton) por consistência com AppDbContext
// — mesmo o gerador não tendo estado próprio hoje, isso evita qualquer
// suposição futura errada sobre tempo de vida se o serviço ganhar
// dependência de algo scoped (como o próprio DbContext) mais adiante.
builder.Services.AddScoped<IOrdemServicoPdfGenerator, OrdemServicoPdfGenerator>();
// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Assistência Técnica API",
        Version = "v1",
        Description = "API para gerenciamento de clientes e ordens de serviço de assistência técnica de PCs e impressoras."
    });
});

var app = builder.Build();

// Obrigatório desde que o QuestPDF adotou o modelo de licenciamento
// (2023.4+): sem isso configurado antes da primeira geração de PDF, a
// chamada real via HTTP lança exceção em runtime. Os testes já
// configuram isso isoladamente (processo de teste é separado do
// processo da API), então esta linha só cobre o app rodando de verdade.
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// Ativa WAL (permite leituras concorrentes durante escrita) e define um
// busy_timeout, para que requisições concorrentes aguardem a liberação do
// lock em vez de falharem imediatamente com "database is locked".
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;");
}

// Pipeline HTTP

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Assistência Técnica API v1");
    });
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Necessário para WebApplicationFactory<Program> nos testes de integração —
// top-level statements geram uma classe Program implícita e internal por
// padrão, invisível para o projeto de testes sem essa declaração explícita.
public partial class Program { }