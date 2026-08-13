using AssistenciaTecnica.Api.Controllers;
using AssistenciaTecnica.Api.Data;
using AssistenciaTecnica.Api.Dtos;
using AssistenciaTecnica.Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AssisTec.Tests;

// Testa a REGRA DE MERGE campo a campo isoladamente, controlando
// exatamente o que ICepLocalizadorService retorna via NSubstitute — sem
// isso, testar o caso "CEP genérico não sobrescreve o que o usuário
// digitou" exigiria depender de um CEP real de Estância nunca mudar de
// comportamento no ViaCEP, o que é frágil para um teste automatizado.
public class ClienteEnderecoResolutionTests
{
    private static AppDbContext CriarContextoEmMemoria() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static (ClienteController Controller, ICepLocalizadorService CepService) CriarController(AppDbContext context)
    {
        var logger = Substitute.For<ILogger<ClienteController>>();
        var cepService = Substitute.For<ICepLocalizadorService>();
        return (new ClienteController(context, logger, cepService), cepService);
    }

    [Fact(DisplayName = "CriarCliente — CEP com endereço completo deve preencher Logradouro e Bairro automaticamente")]
    public async Task CriarCliente_CepComEnderecoCompleto_DevePreencherLogradouroEBairroAutomaticamente()
    {
        await using var context = CriarContextoEmMemoria();
        var (controller, cepService) = CriarController(context);

        cepService.BuscarPorCepAsync("49040-490", Arg.Any<CancellationToken>())
            .Returns(new EnderecoViaCepDto("49040-490", "Rua Simeão Sobral", "Suíssa", "Aracaju", "SE", "2800308"));

        var dto = new CriarClienteDto("Cliente Teste", "79999998888",
            Endereco: new EnderecoDto("49040-490", Numero: "123"));

        var resultado = await controller.CriarCliente(dto);

        var created = Assert.IsType<CreatedAtActionResult>(resultado.Result);
        var cliente = Assert.IsType<ClienteResponseDto>(created.Value);

        cliente.Endereco.Should().NotBeNull();
        cliente.Endereco!.Logradouro.Should().Be("Rua Simeão Sobral");
        cliente.Endereco.Bairro.Should().Be("Suíssa");
        cliente.Endereco.Municipio.Should().Be("Aracaju");
        cliente.Endereco.CodigoMunicipioIbge.Should().Be("2800308");
        cliente.Endereco.Numero.Should().Be("123"); // preservado, nunca vem da API
    }

    [Fact(DisplayName = "CriarCliente — CEP genérico (sem Logradouro/Bairro) deve preservar o que o atendente digitou manualmente")]
    public async Task CriarCliente_CepGenerico_DevePreservarLogradouroEBairroDigitadosManualmente()
    {
        // Reproduz o caso real de Estância/SE: a API confirma o
        // município/UF/IBGE, mas Logradouro e Bairro vêm vazios — o
        // sistema não pode apagar o que o atendente já tinha digitado.
        await using var context = CriarContextoEmMemoria();
        var (controller, cepService) = CriarController(context);

        cepService.BuscarPorCepAsync("49200-000", Arg.Any<CancellationToken>())
            .Returns(new EnderecoViaCepDto("49200-000", "", "", "Estância", "SE", "2802908"));

        var dto = new CriarClienteDto("Cliente Teste", "79999998888",
            Endereco: new EnderecoDto("49200-000", Logradouro: "Rua do Centro", Bairro: "Centro", Numero: "45"));

        var resultado = await controller.CriarCliente(dto);

        var created = Assert.IsType<CreatedAtActionResult>(resultado.Result);
        var cliente = Assert.IsType<ClienteResponseDto>(created.Value);

        cliente.Endereco!.Logradouro.Should().Be("Rua do Centro");
        cliente.Endereco.Bairro.Should().Be("Centro");
        cliente.Endereco.Municipio.Should().Be("Estância"); // esse sim vem da API
        cliente.Endereco.CodigoMunicipioIbge.Should().Be("2802908");
    }

    [Fact(DisplayName = "CriarCliente — CEP não encontrado deve salvar o endereço só com os dados digitados manualmente")]
    public async Task CriarCliente_CepNaoEncontrado_DeveSalvarApenasComDadosManuais()
    {
        await using var context = CriarContextoEmMemoria();
        var (controller, cepService) = CriarController(context);

        cepService.BuscarPorCepAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((EnderecoViaCepDto?)null);

        var dto = new CriarClienteDto("Cliente Teste", "79999998888",
            Endereco: new EnderecoDto("00000-000", Municipio: "Município Digitado", Uf: "SE"));

        var resultado = await controller.CriarCliente(dto);

        var created = Assert.IsType<CreatedAtActionResult>(resultado.Result);
        var cliente = Assert.IsType<ClienteResponseDto>(created.Value);

        cliente.Endereco!.Municipio.Should().Be("Município Digitado");
        cliente.Endereco.CodigoMunicipioIbge.Should().BeNull(); // nunca inventado
    }

    [Fact(DisplayName = "CriarCliente — Sem Endereco informado deve continuar funcionando normalmente (cadastro rápido de balcão)")]
    public async Task CriarCliente_SemEndereco_DeveContinuarFuncionandoNormalmente()
    {
        await using var context = CriarContextoEmMemoria();
        var (controller, cepService) = CriarController(context);

        var dto = new CriarClienteDto("Cliente Teste", "79999998888");

        var resultado = await controller.CriarCliente(dto);

        var created = Assert.IsType<CreatedAtActionResult>(resultado.Result);
        var cliente = Assert.IsType<ClienteResponseDto>(created.Value);

        cliente.Endereco.Should().BeNull();
        await cepService.DidNotReceive().BuscarPorCepAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}