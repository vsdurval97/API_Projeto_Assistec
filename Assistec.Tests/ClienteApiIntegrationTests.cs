using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AssistenciaTecnica.Api.Dtos;
using Xunit;
using Xunit.Abstractions;

namespace AssisTec.Tests;

// Testes de ponta a ponta via HTTP real, contra SQLite real — mesmo padrão
// de OrdemServicoApiIntegrationTests. Fecha a lacuna que ClienteController
// tinha: os testes de controller com EF InMemory provam a lógica, mas
// pulam o ModelState real do [ApiController] e a serialização JSON real.
public class ClienteApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    private static readonly JsonSerializerOptions OpcoesJson = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
    public ClienteApiIntegrationTests(CustomWebApplicationFactory factory, ITestOutputHelper output)
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

    // -----------------------------------------------------------------------
    // POST /api/Cliente — criação
    // -----------------------------------------------------------------------

    [Fact(DisplayName = "POST /api/Cliente — Dados válidos deve retornar 201 Created")]
    public async Task Post_DadosValidos_DeveRetornar201Created()
    {
        var payload = new { nome = $"Cliente Integração {Guid.NewGuid():N}", telefone = "79999998888" };

        var response = await _client.PostAsJsonAsync("/api/Cliente", payload);

        Log("POST com dados válidos", esperado: HttpStatusCode.Created, obtido: response.StatusCode);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var cliente = await response.Content.ReadFromJsonAsync<ClienteResponseDto>(OpcoesJson);
        Log("Nome retornado no corpo da resposta", esperado: payload.nome, obtido: cliente?.Nome);
        Assert.Equal(payload.nome, cliente!.Nome);
        Assert.True(cliente.Id > 0);
    }

    [Fact(DisplayName = "POST /api/Cliente — Nome ausente do payload deve retornar 400 pelo ModelState real")]
    public async Task Post_NomeAusente_DeveRetornar400ViaModelStateReal()
    {
        // Testes de controller (com InMemory) constroem o DTO em C#, onde o
        // parâmetro sempre existe — só um payload JSON real, faltando a
        // propriedade por completo, exercita esse caminho via [Required].
        var payloadSemNome = new { telefone = "79999998888" };

        var response = await _client.PostAsJsonAsync("/api/Cliente", payloadSemNome);

        Log("POST sem 'nome' no corpo", esperado: HttpStatusCode.BadRequest, obtido: response.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory(DisplayName = "POST /api/Cliente — Nome vazio ou abaixo do tamanho mínimo deve retornar 400")]
    [InlineData("")]
    [InlineData("A")] // MinimumLength = 2
    public async Task Post_NomeInvalido_DeveRetornar400ViaModelStateReal(string nomeInvalido)
    {
        var payload = new { nome = nomeInvalido, telefone = "79999998888" };

        var response = await _client.PostAsJsonAsync("/api/Cliente", payload);

        Log($"POST com Nome='{nomeInvalido}'", esperado: HttpStatusCode.BadRequest, obtido: response.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory(DisplayName = "POST /api/Cliente — Telefone vazio ou abaixo do tamanho mínimo deve retornar 400")]
    [InlineData("")]
    [InlineData("123")] // MinimumLength = 8
    public async Task Post_TelefoneInvalido_DeveRetornar400ViaModelStateReal(string telefoneInvalido)
    {
        var payload = new { nome = "Cliente Válido", telefone = telefoneInvalido };

        var response = await _client.PostAsJsonAsync("/api/Cliente", payload);

        Log($"POST com Telefone='{telefoneInvalido}'", esperado: HttpStatusCode.BadRequest, obtido: response.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // -----------------------------------------------------------------------
    // GET /api/Cliente — listagem
    // -----------------------------------------------------------------------

    [Fact(DisplayName = "GET /api/Cliente — Deve retornar 200 OK com o cliente recém-criado na lista")]
    public async Task Get_Todos_DeveRetornar200ComClienteRecemCriadoNaLista()
    {
        var nomeUnico = $"Cliente Listagem {Guid.NewGuid():N}";
        await _client.PostAsJsonAsync("/api/Cliente", new { nome = nomeUnico, telefone = "79988887777" });

        var response = await _client.GetAsync("/api/Cliente");

        Log("GET lista de clientes", esperado: HttpStatusCode.OK, obtido: response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var clientes = await response.Content.ReadFromJsonAsync<List<ClienteResponseDto>>(OpcoesJson);
        Log($"Cliente '{nomeUnico}' presente na lista", esperado: true, obtido: clientes!.Any(c => c.Nome == nomeUnico));
        Assert.Contains(clientes!, c => c.Nome == nomeUnico);
    }

    // -----------------------------------------------------------------------
    // GET /api/Cliente/{id} — busca por id
    // -----------------------------------------------------------------------

    [Fact(DisplayName = "GET /api/Cliente/{id} — Id existente deve retornar 200 OK com os dados corretos")]
    public async Task Get_PorId_IdExistente_DeveRetornar200ComDadosCorretos()
    {
        var criarResponse = await _client.PostAsJsonAsync("/api/Cliente",
            new { nome = $"Cliente Busca {Guid.NewGuid():N}", telefone = "79977776666" });
        var clienteCriado = await criarResponse.Content.ReadFromJsonAsync<ClienteResponseDto>(OpcoesJson);

        var response = await _client.GetAsync($"/api/Cliente/{clienteCriado!.Id}");

        Log($"GET /api/Cliente/{clienteCriado.Id}", esperado: HttpStatusCode.OK, obtido: response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var clienteEncontrado = await response.Content.ReadFromJsonAsync<ClienteResponseDto>(OpcoesJson);
        Assert.Equal(clienteCriado.Nome, clienteEncontrado!.Nome);
    }

    [Fact(DisplayName = "GET /api/Cliente/{id} — Id inexistente deve retornar 404 NotFound")]
    public async Task Get_PorId_IdInexistente_DeveRetornar404()
    {
        var response = await _client.GetAsync("/api/Cliente/999999");

        Log("GET /api/Cliente/999999 (inexistente)", esperado: HttpStatusCode.NotFound, obtido: response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // -----------------------------------------------------------------------
    // PUT /api/Cliente/{id} — atualização
    // -----------------------------------------------------------------------

    [Fact(DisplayName = "PUT /api/Cliente/{id} — Dados válidos deve retornar 200 OK e persistir de fato")]
    public async Task Put_DadosValidos_DeveRetornar200EPersistirDeFato()
    {
        var criarResponse = await _client.PostAsJsonAsync("/api/Cliente",
            new { nome = $"Cliente Original {Guid.NewGuid():N}", telefone = "79966665555" });
        var clienteCriado = await criarResponse.Content.ReadFromJsonAsync<ClienteResponseDto>(OpcoesJson);

        var nomeAtualizado = $"Cliente Atualizado {Guid.NewGuid():N}";
        var putResponse = await _client.PutAsJsonAsync($"/api/Cliente/{clienteCriado!.Id}",
            new { nome = nomeAtualizado, telefone = "79955554444" });

        Log($"PUT /api/Cliente/{clienteCriado.Id}", esperado: HttpStatusCode.OK, obtido: putResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        // Confirma persistência real via uma nova requisição GET independente,
        // não só confiando no corpo devolvido pelo próprio PUT.
        var getResponse = await _client.GetAsync($"/api/Cliente/{clienteCriado.Id}");
        var clienteConfirmado = await getResponse.Content.ReadFromJsonAsync<ClienteResponseDto>(OpcoesJson);

        Log("Nome persistido após consulta independente", esperado: nomeAtualizado, obtido: clienteConfirmado?.Nome);
        Assert.Equal(nomeAtualizado, clienteConfirmado!.Nome);
    }

    [Fact(DisplayName = "PUT /api/Cliente/{id} — Id inexistente deve retornar 404 NotFound")]
    public async Task Put_IdInexistente_DeveRetornar404()
    {
        var response = await _client.PutAsJsonAsync("/api/Cliente/999999",
            new { nome = "Nome Qualquer", telefone = "79900001111" });

        Log("PUT /api/Cliente/999999 (inexistente)", esperado: HttpStatusCode.NotFound, obtido: response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "PUT /api/Cliente/{id} — Nome vazio deve retornar 400 pelo ModelState real")]
    public async Task Put_NomeVazio_DeveRetornar400ViaModelStateReal()
    {
        var criarResponse = await _client.PostAsJsonAsync("/api/Cliente",
            new { nome = $"Cliente Para Editar {Guid.NewGuid():N}", telefone = "79944443333" });
        var clienteCriado = await criarResponse.Content.ReadFromJsonAsync<ClienteResponseDto>(OpcoesJson);

        var response = await _client.PutAsJsonAsync($"/api/Cliente/{clienteCriado!.Id}",
            new { nome = "", telefone = "79944443333" });

        Log("PUT com Nome vazio", esperado: HttpStatusCode.BadRequest, obtido: response.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}