using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AssistenciaTecnica.Api.Dtos;
using FluentAssertions;
using Xunit;

namespace AssisTec.Tests.Pdf;

public class OrdemServicoPdfApiIntegrationTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    // HttpClient/ReadFromJsonAsync não herda a configuração de JSON do
    // Program.cs (AddJsonOptions vale só para o pipeline ASP.NET Core do
    // lado servidor) — sem isso, "tipoEquipamento": "Notebook" (texto)
    // falha ao desserializar contra o enum sem o converter certo.
    private static readonly JsonSerializerOptions OpcoesJson = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private async Task<int> CriarOrdemServicoCompletaAsync()
    {
        var clienteResponse = await _client.PostAsJsonAsync("/api/Cliente", new
        {
            nome = $"Cliente PDF {Guid.NewGuid():N}",
            telefone = "79999998888"
        });
        var cliente = await clienteResponse.Content.ReadFromJsonAsync<ClienteResponseDto>();

        var osResponse = await _client.PostAsJsonAsync("/api/OrdemServico", new
        {
            tipoEquipamento = "Notebook",
            marca = "Dell",
            modelo = "Inspiron 15",
            defeitoRelatado = "Não liga",
            valorMaoDeObra = 150m,
            valorPecas = 80m,
            clienteId = cliente!.Id
        });
        var ordem = await osResponse.Content.ReadFromJsonAsync<OrdemServicoResponseDto>(OpcoesJson); // <- ajuste aqui
        return ordem!.Id;
    }

    [Fact(DisplayName = "GET /api/OrdemServico/{id}/pdf — OS existente deve retornar 200, application/pdf e corpo não vazio")]
    public async Task Get_OsExistente_DeveRetornar200ComPdfValido()
    {
        var id = await CriarOrdemServicoCompletaAsync();

        var response = await _client.GetAsync($"/api/OrdemServico/{id}/pdf");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");

        var bytes = await response.Content.ReadAsByteArrayAsync();
        bytes.Should().NotBeEmpty();
        bytes.Take(4).Should().Equal("%PDF"u8.ToArray());
    }

    [Fact(DisplayName = "GET /api/OrdemServico/{id}/pdf — OS inexistente deve retornar 404, não um PDF")]
    public async Task Get_OsInexistente_DeveRetornar404()
    {
        var response = await _client.GetAsync("/api/OrdemServico/999999/pdf");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Theory(DisplayName = "GET /api/OrdemServico/{id}/pdf — Id zero ou negativo deve retornar 400")]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Get_IdInvalido_DeveRetornar400(int idInvalido)
    {
        var response = await _client.GetAsync($"/api/OrdemServico/{idInvalido}/pdf");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "GET /api/OrdemServico/{id}/pdf — Id não numérico retorna 404 pelo roteamento, não 400")]
    public async Task Get_IdNaoNumerico_DeveRetornar404PeloRoteamento()
    {
        // Não é bug: a rota usa constraint {id:int}, então um valor não
        // numérico não casa com nenhuma rota registrada — comportamento
        // padrão do ASP.NET Core. Documentado aqui para não ser confundido
        // com falha de validação no futuro.
        var response = await _client.GetAsync("/api/OrdemServico/abc/pdf");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}