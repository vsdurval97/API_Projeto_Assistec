using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AssistenciaTecnica.Api.Dtos;
using AssistenciaTecnica.Api.Models;
using Xunit;
using Xunit.Abstractions;

namespace AssisTec.Tests;

// Testes de ponta a ponta via HTTP real, contra SQLite real. Cada [Fact]
// aqui existe para fechar uma lacuna específica que os testes de
// controller (com EF InMemory + chamada direta) não cobrem — não é
// duplicação dos 33 testes existentes, é a camada que falta antes de
// confiar no sistema em produção.
public class OrdemServicoApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    // HttpClient/ReadFromJsonAsync não herda a configuração de JSON do
    // Program.cs (AddJsonOptions vale só para o pipeline ASP.NET Core do
    // lado servidor) — sem isso, "tipoEquipamento": "Computador" (texto)
    // falha ao desserializar contra o enum sem o converter certo.
    private static readonly JsonSerializerOptions OpcoesJson = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public OrdemServicoApiIntegrationTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
    {
        _client = factory.CreateClient();
        _output = output;
    }

    private void Log(string cenario, object esperado, object? obtido)
    {
        _output.WriteLine($"CENÁRIO : {cenario}");
        _output.WriteLine($"ESPERADO: {esperado}");
        _output.WriteLine($"OBTIDO  : {obtido}");
        _output.WriteLine(new string('-', 60));
    }

    // Nomes únicos por teste (Guid) porque a factory mantém o mesmo banco
    // SQLite para todos os testes desta classe — sem isso, um teste
    // interferiria no outro (ex: dois clientes "João" de testes diferentes
    // virando ambiguidade acidental).
    private async Task<int> CriarClienteAsync(string? nomeBase = null)
    {
        var nome = $"{nomeBase ?? "Cliente Teste"} {Guid.NewGuid():N}";
        var response = await _client.PostAsJsonAsync("/api/Cliente", new { nome, telefone = "79900000000" });
        var cliente = await response.Content.ReadFromJsonAsync<ClienteResponseDto>();
        return cliente!.Id;
    }

    [Fact(DisplayName = "POST /api/OrdemServico — TipoEquipamento ausente do JSON deve retornar 400 pelo pipeline real (JsonRequired)")]
    public async Task Post_TipoEquipamentoAusenteDoPayload_DeveRetornar400ViaPipelineReal()
    {
        // Este é o cenário que motivou a correção com [JsonRequired]: os
        // testes de controller não pegam isso porque constroem o DTO em C#
        // (o campo sempre existe); só o desserializador JSON real, agindo
        // sobre um payload que OMITE a propriedade, exercita esse caminho.
        var payloadSemTipoEquipamento = new
        {
            marca = "Dell",
            modelo = "Teste",
            defeitoRelatado = "Não liga",
            valorMaoDeObra = 10m,
            valorPecas = 0m,
            clienteNome = "Alguém"
        };

        var response = await _client.PostAsJsonAsync("/api/OrdemServico", payloadSemTipoEquipamento);

        Log("POST sem 'tipoEquipamento' no corpo", esperado: HttpStatusCode.BadRequest, obtido: response.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory(DisplayName = "POST /api/OrdemServico — Valores negativos devem retornar 400 pelo ModelState real do ApiController")]
    [InlineData(-10, 0)]
    [InlineData(0, -5)]
    public async Task Post_ValoresNegativos_DeveRetornar400ViaModelStateReal(decimal valorMaoDeObra, decimal valorPecas)
    {
        // Substitui, para fins de confiança em produção, o que o
        // ValidarModelo() por reflection só simulava nos testes de controller.
        var clienteId = await CriarClienteAsync();

        var payload = new
        {
            tipoEquipamento = "Computador",
            marca = "Marca",
            modelo = "Modelo",
            defeitoRelatado = "Defeito de teste",
            valorMaoDeObra,
            valorPecas,
            clienteId
        };

        var response = await _client.PostAsJsonAsync("/api/OrdemServico", payload);

        Log($"POST com ValorMaoDeObra={valorMaoDeObra}, ValorPecas={valorPecas}",
            esperado: HttpStatusCode.BadRequest, obtido: response.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "POST /api/OrdemServico — Campo obrigatório vazio deve retornar 400 pelo ModelState real")]
    public async Task Post_MarcaVazia_DeveRetornar400ViaModelStateReal()
    {
        var clienteId = await CriarClienteAsync();

        var payload = new
        {
            tipoEquipamento = "Notebook",
            marca = "",
            modelo = "Modelo válido",
            defeitoRelatado = "Defeito válido",
            valorMaoDeObra = 10m,
            valorPecas = 0m,
            clienteId
        };

        var response = await _client.PostAsJsonAsync("/api/OrdemServico", payload);

        Log("POST com Marca vazia", esperado: HttpStatusCode.BadRequest, obtido: response.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "Ciclo completo via HTTP real — datas de resposta devem vir com sufixo UTC ('Z'), não hora local")]
    public async Task CicloCompleto_ViaHttpReal_DatasDevemVirEmUtcComSufixoZ()
    {
        // Este teste só faz sentido contra SQLite real: EF Core InMemory
        // nunca serializa DateTime para texto e de volta, então nunca
        // exercitaria o bug original (perda de DateTimeKind no round-trip
        // pelo SQLite, corrigido com o ValueConverter no AppDbContext).
        var clienteId = await CriarClienteAsync("Cliente Ciclo Completo");

        var criarResponse = await _client.PostAsJsonAsync("/api/OrdemServico", new
        {
            tipoEquipamento = "Impressora",
            marca = "Epson",
            modelo = "L3250",
            defeitoRelatado = "Não puxa papel",
            valorMaoDeObra = 80m,
            valorPecas = 20m,
            clienteId
        });
        Assert.Equal(HttpStatusCode.Created, criarResponse.StatusCode);

        var jsonCru = await criarResponse.Content.ReadAsStringAsync();
        using var documento = JsonDocument.Parse(jsonCru);
        var dataAberturaTexto = documento.RootElement.GetProperty("dataAbertura").GetString();

        Log("Sufixo do campo dataAbertura no JSON bruto de resposta",
            esperado: "termina com 'Z' (UTC)", obtido: dataAberturaTexto);
        Assert.EndsWith("Z", dataAberturaTexto);

        // Em CicloCompleto_ViaHttpReal_DatasDevemVirEmUtcComSufixoZ:
        var ordem = await criarResponse.Content.ReadFromJsonAsync<OrdemServicoResponseDto>(OpcoesJson);
        var id = ordem!.Id;

        // Avança o ciclo completo e confirma o mesmo comportamento em cada etapa
        var paraEmAnalise = await _client.PutAsJsonAsync($"/api/OrdemServico/{id}/status", new { status = "EmAnalise" });
        Assert.Equal(HttpStatusCode.OK, paraEmAnalise.StatusCode);

        var paraPronto = await _client.PutAsJsonAsync($"/api/OrdemServico/{id}/status", new { status = "Pronto" });
        var prontoJson = await paraPronto.Content.ReadAsStringAsync();
        using var prontoDoc = JsonDocument.Parse(prontoJson);
        var dataConclusaoTexto = prontoDoc.RootElement.GetProperty("dataConclusao").GetString();

        Log("Sufixo do campo dataConclusao após transição para Pronto",
            esperado: "termina com 'Z' (UTC)", obtido: dataConclusaoTexto);
        Assert.EndsWith("Z", dataConclusaoTexto);

        var paraEntregue = await _client.PutAsJsonAsync($"/api/OrdemServico/{id}/status", new { status = "Entregue" });
        Assert.Equal(HttpStatusCode.OK, paraEntregue.StatusCode);

        var bloqueado = await _client.PutAsJsonAsync($"/api/OrdemServico/{id}/status", new { status = "EmAnalise" });
        Log("Tentativa de regredir OS já Entregue", esperado: HttpStatusCode.BadRequest, obtido: bloqueado.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, bloqueado.StatusCode);
    }

    [Fact(DisplayName = "POST /api/OrdemServico — ClienteNome ambíguo via HTTP real deve retornar 400 com candidatos")]
    public async Task Post_ClienteNomeAmbiguo_ViaHttpReal_DeveRetornarCandidatos()
    {
        var nomeComum = $"Homônimo {Guid.NewGuid():N}";
        await _client.PostAsJsonAsync("/api/Cliente", new { nome = nomeComum, telefone = "79911111111" });
        await _client.PostAsJsonAsync("/api/Cliente", new { nome = nomeComum, telefone = "79922222222" });

        var response = await _client.PostAsJsonAsync("/api/OrdemServico", new
        {
            tipoEquipamento = "Outros",
            marca = "Marca",
            modelo = "Modelo",
            defeitoRelatado = "Defeito",
            valorMaoDeObra = 0m,
            valorPecas = 0m,
            clienteNome = nomeComum
        });

        Log("POST com ClienteNome ambíguo via HTTP real",
            esperado: HttpStatusCode.BadRequest, obtido: response.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains("clientesEncontrados", corpo);
    }
}