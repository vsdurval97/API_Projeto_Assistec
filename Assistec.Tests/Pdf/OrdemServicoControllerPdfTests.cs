/*using AssistenciaTecnica.Api.Controllers;
using AssistenciaTecnica.Api.Data;
using AssistenciaTecnica.Api.Dtos;
using AssistenciaTecnica.Api.Models;
using AssistenciaTecnica.Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace AssisTec.Tests.Pdf;

// Isola a DECISÃO do controller (400/404/200) do QuestPDF real — o
// gerador é substituído (NSubstitute), então "gerar PDF de verdade nunca
// roda aqui". A renderização real é responsabilidade exclusiva de
// OrdemServicoPdfGeneratorTests, não deste arquivo.
public class OrdemServicoControllerPdfTests
{
    private static AppDbContext CriarContextoEmMemoria() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static (OrdemServicoController Controller, IOrdemServicoPdfGenerator Gerador) CriarController(AppDbContext context)
    {
        var gerador = Substitute.For<IOrdemServicoPdfGenerator>();
        var logger = Substitute.For<ILogger<OrdemServicoController>>();
        return (new OrdemServicoController(context, logger, gerador), gerador);
    }

    [Fact(DisplayName = "GerarPdf — Id inválido (<= 0) deve retornar 400 sem consultar banco nem gerador")]
    public async Task GerarPdf_IdInvalido_DeveRetornar400SemChamarGerador()
    {
        await using var context = CriarContextoEmMemoria();
        var (controller, gerador) = CriarController(context);

        var resultado = await controller.GerarPdf(0);

        resultado.Should().BeOfType<BadRequestObjectResult>();
        gerador.DidNotReceive().Gerar(Arg.Any<OrdemServicoPdfDto>());
    }

    [Fact(DisplayName = "GerarPdf — OS inexistente deve retornar 404 sem chamar o gerador")]
    public async Task GerarPdf_OsInexistente_DeveRetornar404SemChamarGerador()
    {
        await using var context = CriarContextoEmMemoria();
        var (controller, gerador) = CriarController(context);

        var resultado = await controller.GerarPdf(999);

        resultado.Should().BeOfType<NotFoundObjectResult>();
        gerador.DidNotReceive().Gerar(Arg.Any<OrdemServicoPdfDto>());
    }

    [Fact(DisplayName = "GerarPdf — OS existente deve retornar 200 com Content-Type application/pdf e os bytes do gerador")]
    public async Task GerarPdf_OsExistente_DeveRetornarArquivoComBytesDoGerador()
    {
        await using var context = CriarContextoEmMemoria();
        var cliente = new Cliente { Nome = "Maria", Telefone = "79988887777", Documento = "12345678900" };
        var ordem = new OrdemServico
        {
            TipoEquipamento = TipoEquipamento.Impressora,
            Marca = "Epson",
            Modelo = "L3250",
            DefeitoRelatado = "Não liga",
            ValorMaoDeObra = 50m,
            ValorPecas = 0m,
            Cliente = cliente
        };
        context.Clientes.Add(cliente);
        context.OrdensServico.Add(ordem);
        await context.SaveChangesAsync();

        var (controller, gerador) = CriarController(context);
        byte[] bytesFalsos = [0x25, 0x50, 0x44, 0x46]; // assinatura "%PDF" — não precisa ser um PDF real aqui
        gerador.Gerar(Arg.Any<OrdemServicoPdfDto>()).Returns(bytesFalsos);

        var resultado = await controller.GerarPdf(ordem.Id);

        var arquivo = resultado.Should().BeOfType<FileContentResult>().Subject;
        arquivo.ContentType.Should().Be("application/pdf");
        arquivo.FileContents.Should().Equal(bytesFalsos);
        gerador.Received(1).Gerar(Arg.Any<OrdemServicoPdfDto>());
    }
}*/