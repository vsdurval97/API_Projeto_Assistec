using AssistenciaTecnica.Api.Controllers;
using AssistenciaTecnica.Api.Data;
using AssistenciaTecnica.Api.Dtos;
using AssistenciaTecnica.Api.Models;
using AssistenciaTecnica.Api.Services; 
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace AssisTec.Tests;

public class ClienteControllerTests : TesteBase
{
    public ClienteControllerTests(ITestOutputHelper output) : base(output)
    {
    }

    private static ClienteController CriarController(AppDbContext context)
    {
        var loggerFalso = Substitute.For<ILogger<ClienteController>>();
        // NSubstitute retorna Task<EnderecoViaCepDto?> completo com null
        // por padrão para métodos async não configurados — suficiente
        // para os testes existentes, que não envolvem Endereco.
        var cepLocalizadorFalso = Substitute.For<ICepLocalizadorService>();
        return new ClienteController(context, loggerFalso, cepLocalizadorFalso);
    }

    [Fact(DisplayName = "POST /cliente — Dados válidos deve retornar 201 Created com o DTO correto")]
    public async Task CriarCliente_DadosValidos_DeveRetornar201ComDtoCorreto()
    {
        // Arrange
        await using var context = CriarContextoEmMemoria();
        var controller = CriarController(context);
        var dto = new CriarClienteDto("Ana Beatriz Costa", "79991234567");

        // Act
        var resultado = await controller.CriarCliente(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(resultado.Result);
        Log("Criar cliente com dados válidos",
            esperado: StatusCodes.Status201Created, obtido: createdResult.StatusCode);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);

        var clienteCriado = Assert.IsType<ClienteResponseDto>(createdResult.Value);
        Log("Nome do cliente retornado", esperado: dto.Nome, obtido: clienteCriado.Nome);
        Assert.Equal(dto.Nome, clienteCriado.Nome);

        var totalNoBanco = await context.Clientes.CountAsync();
        Log("Clientes persistidos no banco", esperado: 1, obtido: totalNoBanco);
        Assert.Equal(1, totalNoBanco);
    }

    [Fact(DisplayName = "GET /cliente/{id} — ID inexistente deve retornar 404 NotFound")]
    public async Task BuscarPorId_IdInexistente_DeveRetornarNotFound()
    {
        // Arrange
        await using var context = CriarContextoEmMemoria();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.BuscarPorId(9999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(resultado.Result);
        Log("Buscar cliente com Id=9999 (inexistente)",
            esperado: StatusCodes.Status404NotFound, obtido: notFoundResult.StatusCode);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact(DisplayName = "PUT /cliente/{id} — Atualização válida deve retornar 200 OK e persistir as alterações")]
    public async Task AtualizarCliente_DadosValidos_DeveRetornar200EPersistirAlteracoes()
    {
        // Arrange
        await using var context = CriarContextoEmMemoria();
        var clienteExistente = new Cliente { Nome = "Bruno Alves", Telefone = "79988887777" };
        context.Clientes.Add(clienteExistente);
        await context.SaveChangesAsync();

        var controller = CriarController(context);
        var dto = new AtualizarClienteDto("Bruno Alves Silva", "79999998888");

        // Act
        var resultado = await controller.AtualizarCliente(clienteExistente.Id, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
        Log("Atualizar cliente existente com dados válidos",
            esperado: StatusCodes.Status200OK, obtido: okResult.StatusCode);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var clienteNoBanco = await context.Clientes.AsNoTracking().FirstAsync(c => c.Id == clienteExistente.Id);
        Log("Nome persistido no banco após atualização",
            esperado: "Bruno Alves Silva", obtido: clienteNoBanco.Nome);
        Assert.Equal("Bruno Alves Silva", clienteNoBanco.Nome);
    }

    [Fact(DisplayName = "PUT /cliente/{id} — ID inexistente deve retornar 404 NotFound e não alterar o banco")]
    public async Task AtualizarCliente_IdInexistente_DeveRetornarNotFound()
    {
        // Arrange
        await using var context = CriarContextoEmMemoria();
        var controller = CriarController(context);
        var dto = new AtualizarClienteDto("Nome Qualquer", "79900001111");

        // Act
        var resultado = await controller.AtualizarCliente(9999, dto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(resultado.Result);
        Log("Atualizar cliente com Id=9999 (inexistente)",
            esperado: StatusCodes.Status404NotFound, obtido: notFoundResult.StatusCode);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);

        var totalNoBanco = await context.Clientes.CountAsync();
        Log("Clientes criados indevidamente", esperado: 0, obtido: totalNoBanco);
        Assert.Equal(0, totalNoBanco);
    }
}