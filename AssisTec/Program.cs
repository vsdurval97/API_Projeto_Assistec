using AssistenciaTecnica.Api.Data;
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