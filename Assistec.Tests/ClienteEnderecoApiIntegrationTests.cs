// AssisTec.Tests/ClienteEnderecoApiIntegrationTests.cs
using AssisTec.Tests;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AssisTec.Tests.Fakes;
using AssistenciaTecnica.Api.Dtos;
using FluentAssertions;
using Xunit;

namespace AssisTec.Tests;

// Ponta a ponta via HTTP real, resolução de CEP via FakeCepLocalizadorService
// (registrado em CustomWebApplicationFactory) — nunca toca o ViaCEP real.
public class ClienteEnderecoApiIntegrationTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly JsonSerializerOptions OpcoesJson = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact(DisplayName = "POST /api/Cliente — CEP com endereço completo deve retornar Logradouro e Bairro preenchidos")]
    public async Task Post_CepComEnderecoCompleto_DeveRetornarEnderecoPreenchido()
    {
        var response = await _client.PostAsJsonAsync("/api/Cliente", new
        {
            nome = $"Cliente CEP Completo {Guid.NewGuid():N}",
            telefone = "79999998888",
            endereco = new { cep = FakeCepLocalizadorService.CepComEnderecoCompleto, numero = "100" }
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var cliente = await response.Content.ReadFromJsonAsync<ClienteResponseDto>(OpcoesJson);
        cliente!.Endereco!.Logradouro.Should().Be("Rua Simeão Sobral");
        cliente.Endereco.Municipio.Should().Be("Aracaju");
        cliente.Endereco.Numero.Should().Be("100");
    }

    [Fact(DisplayName = "POST /api/Cliente — CEP genérico deve preservar Logradouro/Bairro digitados manualmente")]
    public async Task Post_CepGenerico_DevePreservarDadosDigitadosManualmente()
    {
        var response = await _client.PostAsJsonAsync("/api/Cliente", new
        {
            nome = $"Cliente CEP Genérico {Guid.NewGuid():N}",
            telefone = "79999998888",
            endereco = new
            {
                cep = FakeCepLocalizadorService.CepGenerico,
                logradouro = "Rua do Centro",
                bairro = "Centro",
                numero = "45"
            }
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var cliente = await response.Content.ReadFromJsonAsync<ClienteResponseDto>(OpcoesJson);
        cliente!.Endereco!.Logradouro.Should().Be("Rua do Centro");
        cliente.Endereco.Bairro.Should().Be("Centro");
        cliente.Endereco.Municipio.Should().Be("Estância");
    }

    [Fact(DisplayName = "POST /api/Cliente — Sem Endereco continua criando cliente normalmente")]
    public async Task Post_SemEndereco_DeveCriarClienteNormalmente()
    {
        var response = await _client.PostAsJsonAsync("/api/Cliente", new
        {
            nome = $"Cliente Sem Endereço {Guid.NewGuid():N}",
            telefone = "79999998888"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var cliente = await response.Content.ReadFromJsonAsync<ClienteResponseDto>(OpcoesJson);
        cliente!.Endereco.Should().BeNull();
    }

    [Fact(DisplayName = "POST /api/Cliente — CEP em formato inválido deve retornar 400 pelo ModelState real")]
    public async Task Post_CepFormatoInvalido_DeveRetornar400()
    {
        var response = await _client.PostAsJsonAsync("/api/Cliente", new
        {
            nome = $"Cliente CEP Inválido {Guid.NewGuid():N}",
            telefone = "79999998888",
            endereco = new { cep = "123" }
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}