using System.Net;
using AssistenciaTecnica.Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AssisTec.Tests.Cep;

public class CepLocalizadorServiceTests
{
    private static CepLocalizadorService CriarServico(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var httpClient = new HttpClient(new FakeHttpMessageHandler(responder))
        {
            BaseAddress = new Uri("https://viacep.com.br/ws/")
        };
        var loggerFalso = Substitute.For<ILogger<CepLocalizadorService>>();
        return new CepLocalizadorService(httpClient, loggerFalso);
    }
    [Fact(DisplayName = "BuscarPorCepAsync — CEP válido com endereço completo deve retornar todos os campos")]
    public async Task BuscarPorCepAsync_CepValidoComEnderecoCompleto_DeveRetornarTodosOsCampos()
    {
        const string jsonViaCep = """
            {
                "cep": "49040-490",
                "logradouro": "Rua Simeão Sobral",
                "bairro": "Suíssa",
                "localidade": "Aracaju",
                "uf": "SE",
                "ibge": "2800308"
            }
            """;
        var servico = CriarServico(_ => FakeHttpMessageHandler.RespostaJson(HttpStatusCode.OK, jsonViaCep));

        var resultado = await servico.BuscarPorCepAsync("49040-490");

        resultado.Should().NotBeNull();
        resultado!.Logradouro.Should().Be("Rua Simeão Sobral");
        resultado.Bairro.Should().Be("Suíssa");
        resultado.Localidade.Should().Be("Aracaju");
        resultado.Uf.Should().Be("SE");
        resultado.Ibge.Should().Be("2800308");
    }

    [Fact(DisplayName = "BuscarPorCepAsync — CEP genérico de cidade pequena (Logradouro/Bairro vazios) deve retornar mesmo assim, sem tratar como erro")]
    public async Task BuscarPorCepAsync_CepGenericoSemLogradouro_DeveRetornarComCamposVazios()
    {
        // Reproduz o caso real de Estância/SE: município inteiro com um
        // único CEP, sem granularidade de rua. O ViaCEP devolve
        // logradouro/bairro como string vazia, não como erro — o serviço
        // não deve confundir "campo vazio" com "CEP inexistente".
        const string jsonCepGenerico = """
            {
                "cep": "49200-000",
                "logradouro": "",
                "bairro": "",
                "localidade": "Estância",
                "uf": "SE",
                "ibge": "2802908"
            }
            """;
        var servico = CriarServico(_ => FakeHttpMessageHandler.RespostaJson(HttpStatusCode.OK, jsonCepGenerico));

        var resultado = await servico.BuscarPorCepAsync("49200-000");

        resultado.Should().NotBeNull();
        resultado!.Logradouro.Should().BeEmpty();
        resultado.Bairro.Should().BeEmpty();
        resultado.Localidade.Should().Be("Estância");
        resultado.Uf.Should().Be("SE");
        resultado.Ibge.Should().Be("2802908");
    }

    [Fact(DisplayName = "BuscarPorCepAsync — CEP inexistente (ViaCEP retorna erro:true) deve retornar null")]
    public async Task BuscarPorCepAsync_CepInexistente_DeveRetornarNull()
    {
        // Particularidade do ViaCEP: CEP inexistente não retorna 404 HTTP,
        // retorna 200 OK com {"erro": true} no corpo. Um parsing ingênuo
        // que só checasse o status code passaria batido por esse caso.
        const string jsonErro = """{"erro": true}""";
        var servico = CriarServico(_ => FakeHttpMessageHandler.RespostaJson(HttpStatusCode.OK, jsonErro));

        var resultado = await servico.BuscarPorCepAsync("00000-000");

        resultado.Should().BeNull();
    }

    [Theory(DisplayName = "BuscarPorCepAsync — CEP com formato inválido deve retornar null sem chamar a API")]
    [InlineData("123")]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("abcdefgh")]
    public async Task BuscarPorCepAsync_FormatoInvalido_DeveRetornarNullSemChamarApi(string? cepInvalido)
    {
        var chamouApi = false;
        var servico = CriarServico(_ =>
        {
            chamouApi = true;
            return FakeHttpMessageHandler.RespostaJson(HttpStatusCode.OK, "{}");
        });

        var resultado = await servico.BuscarPorCepAsync(cepInvalido!);

        resultado.Should().BeNull();
        // Validar o formato ANTES de sair para a rede evita uma chamada
        // HTTP desperdiçada para um CEP que já se sabe ser inválido —
        // mais rápido para o atendente e mais gentil com a API externa.
        chamouApi.Should().BeFalse();
    }

    [Fact(DisplayName = "BuscarPorCepAsync — CEP com máscara (hífen) deve ser normalizado antes de consultar")]
    public async Task BuscarPorCepAsync_CepComMascara_DeveNormalizarAntesDeConsultar()
    {
        string? cepRequisitado = null;
        var servico = CriarServico(request =>
        {
            cepRequisitado = request.RequestUri!.ToString();
            return FakeHttpMessageHandler.RespostaJson(HttpStatusCode.OK,
                """{"cep":"49040-490","logradouro":"Rua X","bairro":"Bairro Y","localidade":"Aracaju","uf":"SE","ibge":"2800308"}""");
        });

        await servico.BuscarPorCepAsync("49040-490"); // com hífen

        // ViaCEP aceita o CEP sem máscara na URL — normalizar aqui evita
        // depender de o atendente digitar exatamente no formato esperado.
        cepRequisitado.Should().Contain("49040490");
        cepRequisitado.Should().NotContain("-");
    }

    [Fact(DisplayName = "BuscarPorCepAsync — Erro de rede (exceção HTTP) deve retornar null, nunca lançar")]
    public async Task BuscarPorCepAsync_ErroDeRede_DeveRetornarNullSemLancar()
    {
        var servico = CriarServico(_ => throw new HttpRequestException("Falha de conexão simulada"));

        var act = async () => await servico.BuscarPorCepAsync("49040-490");

        // Cadastro de cliente não pode falhar porque uma API externa caiu
        // — o preenchimento automático é um "bônus", nunca um requisito
        // bloqueante do fluxo principal.
        await act.Should().NotThrowAsync();
        (await servico.BuscarPorCepAsync("49040-490")).Should().BeNull();
    }

    [Fact(DisplayName = "BuscarPorCepAsync — Resposta HTTP de erro (500) deve retornar null, nunca lançar")]
    public async Task BuscarPorCepAsync_RespostaHttpErro_DeveRetornarNullSemLancar()
    {
        var servico = CriarServico(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var resultado = await servico.BuscarPorCepAsync("49040-490");

        resultado.Should().BeNull();
    }

    [Fact(DisplayName = "BuscarPorCepAsync — JSON malformado na resposta deve retornar null, nunca lançar")]
    public async Task BuscarPorCepAsync_JsonMalformado_DeveRetornarNullSemLancar()
    {
        var servico = CriarServico(_ => FakeHttpMessageHandler.RespostaJson(HttpStatusCode.OK, "não é um json válido"));

        var act = async () => await servico.BuscarPorCepAsync("49040-490");

        await act.Should().NotThrowAsync();
    }
}