// AssisTec.Tests/ClienteControllerTests.cs
using AssistenciaTecnica.Api.Controllers;
using AssistenciaTecnica.Api.Data;
using AssistenciaTecnica.Api.Dtos;
using AssistenciaTecnica.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AssisTec.Tests;

public class ClienteControllerTests
{
    // Cria um AppDbContext isolado (banco InMemory único por teste),
    // evitando que o estado de um teste vaze para o outro.
    private static AppDbContext CriarContextoEmMemoria()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static ClienteController CriarController(AppDbContext context)
    {
        var loggerFalso = Substitute.For<ILogger<ClienteController>>();
        return new ClienteController(context, loggerFalso);
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
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);

        var clienteCriado = Assert.IsType<ClienteResponseDto>(createdResult.Value);
        Assert.Equal(dto.Nome, clienteCriado.Nome);
        Assert.Equal(dto.Telefone, clienteCriado.Telefone);
        Assert.True(clienteCriado.Id > 0);

        // Confirma que a persistência realmente ocorreu no banco
        var totalNoBanco = await context.Clientes.CountAsync();
        Assert.Equal(1, totalNoBanco);
    }

    [Fact(DisplayName = "GET /cliente/{id} — ID inexistente deve retornar 404 NotFound")]
    public async Task BuscarPorId_IdInexistente_DeveRetornarNotFound()
    {
        // Arrange
        await using var context = CriarContextoEmMemoria(); // banco vazio
        var controller = CriarController(context);

        // Act
        var resultado = await controller.BuscarPorId(9999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(resultado.Result);
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
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var clienteAtualizado = Assert.IsType<ClienteResponseDto>(okResult.Value);
        Assert.Equal(dto.Nome, clienteAtualizado.Nome);
        Assert.Equal(dto.Telefone, clienteAtualizado.Telefone);

        // Confirma que a alteração foi realmente persistida no banco,
        // consultando de forma independente do resultado retornado pela action.
        var clienteNoBanco = await context.Clientes
            .AsNoTracking()
            .FirstAsync(c => c.Id == clienteExistente.Id);

        Assert.Equal("Bruno Alves Silva", clienteNoBanco.Nome);
        Assert.Equal("79999998888", clienteNoBanco.Telefone);
    }

    [Fact(DisplayName = "PUT /cliente/{id} — ID inexistente deve retornar 404 NotFound e não alterar o banco")]
    public async Task AtualizarCliente_IdInexistente_DeveRetornarNotFound()
    {
        // Arrange
        await using var context = CriarContextoEmMemoria(); // banco vazio
        var controller = CriarController(context);

        var dto = new AtualizarClienteDto("Nome Qualquer", "79900001111");

        // Act
        var resultado = await controller.AtualizarCliente(9999, dto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(resultado.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);

        // Garante que nada foi criado indevidamente
        var totalNoBanco = await context.Clientes.CountAsync();
        Assert.Equal(0, totalNoBanco);
    }
}