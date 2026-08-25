// AssisTec.Tests/OrdemServicoControllerTests.cs
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using AssistenciaTecnica.Api.Controllers;
using AssistenciaTecnica.Api.Data;
using AssistenciaTecnica.Api.Dtos;
using AssistenciaTecnica.Api.Models;
using AssistenciaTecnica.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace AssisTec.Tests;

public class OrdemServicoControllerTests : TesteBase
{
    public OrdemServicoControllerTests(ITestOutputHelper output) : base(output)
    {
    }

    // -----------------------------------------------------------------------
    // Helpers específicos deste arquivo (CriarContextoEmMemoria e Log agora
    // vêm de TesteBase — ver TesteBase.cs)
    // -----------------------------------------------------------------------

    private static OrdemServicoController CriarController(AppDbContext context)
    {
        var loggerFalso = Substitute.For<ILogger<OrdemServicoController>>();
        var pdfGeneratorFalso = Substitute.For<IOrdemServicoPdfGenerator>();
        return new OrdemServicoController(context, loggerFalso, pdfGeneratorFalso);
    }

    // Simula a validação automática de ModelState que o [ApiController] faz
    // no pipeline real. Em records posicionais (C# 10+), os atributos de
    // validação ficam anexados ao PARÂMETRO do construtor primário — por
    // isso lemos via reflection nos parâmetros do construtor, e não via
    // Validator.TryValidateObject (que só enxerga atributos em propriedades).
    private static bool ValidarModelo<T>(T modelo, ControllerBase controller) where T : notnull
    {
        var tipo = typeof(T);
        var construtor = tipo.GetConstructors().First();
        var parametros = construtor.GetParameters();

        bool valido = true;

        foreach (var parametro in parametros)
        {
            var propriedade = tipo.GetProperty(parametro.Name!, BindingFlags.Public | BindingFlags.Instance);
            var valor = propriedade?.GetValue(modelo);

            var atributosValidacao = parametro
                .GetCustomAttributes(typeof(ValidationAttribute), inherit: true)
                .Cast<ValidationAttribute>();

            foreach (var atributo in atributosValidacao)
            {
                if (!atributo.IsValid(valor))
                {
                    valido = false;
                    controller.ModelState.AddModelError(
                        parametro.Name ?? string.Empty,
                        atributo.ErrorMessage ?? "Valor inválido.");
                }
            }
        }

        return valido;
    }

    private static async Task<Cliente> CriarClienteAsync(AppDbContext context, string nome, string telefone = "79900000000")
    {
        var cliente = new Cliente { Nome = nome, Telefone = telefone };
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();
        return cliente;
    }

    // Cria uma OS já em "Pronto" (avançando a máquina de estados
    // diretamente na entidade, sem passar pelo controller) — ponto de
    // partida de todo teste de AtualizarStatus(..., Entregue), já que
    // Recebido -> Entregue não é uma transição válida.
    private static async Task<OrdemServico> CriarOrdemProntaAsync(
        AppDbContext context, decimal valorMaoDeObra, decimal valorPecas)
    {
        var cliente = await CriarClienteAsync(context, $"Cliente Pagamento {Guid.NewGuid():N}");

        var ordem = new OrdemServico
        {
            TipoEquipamento = TipoEquipamento.Computador,
            Marca = "Marca",
            Modelo = "Modelo",
            DefeitoRelatado = "Defeito para teste de pagamento",
            ValorMaoDeObra = valorMaoDeObra,
            ValorPecas = valorPecas,
            ClienteId = cliente.Id
        };
        context.OrdensServico.Add(ordem);
        await context.SaveChangesAsync();

        ordem.AtualizarStatus(StatusOrdemServico.EmAnalise);
        ordem.AtualizarStatus(StatusOrdemServico.Pronto);
        await context.SaveChangesAsync();

        return ordem;
    }

    // -------------------------------------------------------------------
    // AtualizarStatus + Pagamentos — só é exigido (e só é aceito) na
    // transição para Entregue; ver OrdemServico.TentarRegistrarPagamentos
    // para a regra de negócio em si, testada isoladamente em
    // OrdemServicoTests.cs. Aqui o foco é a orquestração HTTP: qual status
    // o controller devolve para cada resultado.
    // -------------------------------------------------------------------

    [Fact(DisplayName = "PUT status — Marcar como Entregue sem informar Pagamentos deve retornar 400")]
    public async Task AtualizarStatus_EntregueSemPagamentos_DeveRetornar400()
    {
        await using var context = CriarContextoEmMemoria();
        var ordem = await CriarOrdemProntaAsync(context, valorMaoDeObra: 80m, valorPecas: 20m);
        var controller = CriarController(context);

        var dto = new AtualizarStatusDto(StatusOrdemServico.Entregue); // Pagamentos = null (default)
        var resultado = await controller.AtualizarStatus(ordem.Id, dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado.Result);
        Log("PUT status Entregue sem Pagamentos", esperado: StatusCodes.Status400BadRequest, obtido: badRequest.StatusCode);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);

        var statusNoBanco = (await context.OrdensServico.FindAsync(ordem.Id))!.Status;
        Log("Status da OS no banco após rejeição", esperado: StatusOrdemServico.Pronto, obtido: statusNoBanco);
        Assert.Equal(StatusOrdemServico.Pronto, statusNoBanco);
    }

    [Fact(DisplayName = "PUT status — Marcar como Entregue com lista de Pagamentos vazia deve retornar 400")]
    public async Task AtualizarStatus_EntregueComPagamentosVazio_DeveRetornar400()
    {
        await using var context = CriarContextoEmMemoria();
        var ordem = await CriarOrdemProntaAsync(context, valorMaoDeObra: 80m, valorPecas: 20m);
        var controller = CriarController(context);

        var dto = new AtualizarStatusDto(StatusOrdemServico.Entregue, Pagamentos: new List<PagamentoDto>());
        var resultado = await controller.AtualizarStatus(ordem.Id, dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado.Result);
        Log("PUT status Entregue com Pagamentos = []", esperado: StatusCodes.Status400BadRequest, obtido: badRequest.StatusCode);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
    }

    [Fact(DisplayName = "PUT status — Marcar como Entregue com soma de Pagamentos divergente do ValorTotal deve retornar 400")]
    public async Task AtualizarStatus_EntregueComSomaDivergente_DeveRetornar400()
    {
        await using var context = CriarContextoEmMemoria();
        var ordem = await CriarOrdemProntaAsync(context, valorMaoDeObra: 80m, valorPecas: 20m); // total = 100
        var controller = CriarController(context);

        var dto = new AtualizarStatusDto(
            StatusOrdemServico.Entregue,
            Pagamentos: new List<PagamentoDto> { new(FormaPgto.Dinheiro, 90m) }); // falta 10

        var resultado = await controller.AtualizarStatus(ordem.Id, dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(resultado.Result);
        Log("PUT status Entregue com soma=90 e ValorTotal=100",
            esperado: StatusCodes.Status400BadRequest, obtido: badRequest.StatusCode);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);

        var statusNoBanco = (await context.OrdensServico.FindAsync(ordem.Id))!.Status;
        Log("Status da OS no banco após rejeição", esperado: StatusOrdemServico.Pronto, obtido: statusNoBanco);
        Assert.Equal(StatusOrdemServico.Pronto, statusNoBanco);
    }

    [Fact(DisplayName = "PUT status — Marcar como Entregue com Pagamentos cobrindo o ValorTotal deve retornar 200 e persistir os pagamentos")]
    public async Task AtualizarStatus_EntregueComPagamentosValidos_DeveRetornar200EPersistirPagamentos()
    {
        await using var context = CriarContextoEmMemoria();
        var ordem = await CriarOrdemProntaAsync(context, valorMaoDeObra: 60m, valorPecas: 40m); // total = 100
        var controller = CriarController(context);

        var dto = new AtualizarStatusDto(
            StatusOrdemServico.Entregue,
            Pagamentos:
            [
                new PagamentoDto(FormaPgto.PagamentoInstantaneoPix, 70m),
            new PagamentoDto(FormaPgto.Dinheiro, 30m)
            ]);

        var resultado = await controller.AtualizarStatus(ordem.Id, dto);

        var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
        Log("PUT status Entregue com Pix(70)+Dinheiro(30)=ValorTotal(100)",
            esperado: StatusCodes.Status200OK, obtido: okResult.StatusCode);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var resposta = Assert.IsType<OrdemServicoResponseDto>(okResult.Value);
        Log("Status retornado na resposta", esperado: StatusOrdemServico.Entregue, obtido: resposta.Status);
        Assert.Equal(StatusOrdemServico.Entregue, resposta.Status);

        Log("Quantidade de pagamentos na resposta", esperado: 2, obtido: resposta.Pagamentos.Count);
        Assert.Equal(2, resposta.Pagamentos.Count);

        var pagamentosNoBanco = await context.Pagamentos.Where(p => p.OrdemServicoId == ordem.Id).CountAsync();
        Log("Pagamentos persistidos no banco", esperado: 2, obtido: pagamentosNoBanco);
        Assert.Equal(2, pagamentosNoBanco);
    }

    [Fact(DisplayName = "PUT status — Transições diferentes de Entregue não exigem Pagamentos")]
    public async Task AtualizarStatus_TransicaoDiferenteDeEntregue_NaoExigePagamentos()
    {
        await using var context = CriarContextoEmMemoria();
        var cliente = await CriarClienteAsync(context, $"Cliente Sem Pagamento {Guid.NewGuid():N}");
        var ordem = new OrdemServico
        {
            TipoEquipamento = TipoEquipamento.Notebook,
            Marca = "Marca",
            Modelo = "Modelo",
            DefeitoRelatado = "Defeito qualquer",
            ValorMaoDeObra = 10m,
            ValorPecas = 0m,
            ClienteId = cliente.Id
        };
        context.OrdensServico.Add(ordem);
        await context.SaveChangesAsync();

        var controller = CriarController(context);
        var dto = new AtualizarStatusDto(StatusOrdemServico.EmAnalise); // Pagamentos = null

        var resultado = await controller.AtualizarStatus(ordem.Id, dto);

        var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
        Log("PUT status Recebido -> EmAnalise sem Pagamentos",
            esperado: StatusCodes.Status200OK, obtido: okResult.StatusCode);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);
    }

    [Fact(DisplayName = "PUT status — Pagamentos informados em transição que não é Entregue devem ser ignorados")]
    public async Task AtualizarStatus_PagamentosInformadosForaDeEntregue_DevemSerIgnorados()
    {
        await using var context = CriarContextoEmMemoria();
        var cliente = await CriarClienteAsync(context, $"Cliente Pagamento Ignorado {Guid.NewGuid():N}");
        var ordem = new OrdemServico
        {
            TipoEquipamento = TipoEquipamento.Notebook,
            Marca = "Marca",
            Modelo = "Modelo",
            DefeitoRelatado = "Defeito qualquer",
            ValorMaoDeObra = 10m,
            ValorPecas = 0m,
            ClienteId = cliente.Id
        };
        context.OrdensServico.Add(ordem);
        await context.SaveChangesAsync();

        var controller = CriarController(context);

        // Pagamento não faz sentido nenhum nesta transição (nem soma com o
        // total) — se o controller ignorar corretamente, isso não deve
        // impedir a transição nem ser persistido.
        var dto = new AtualizarStatusDto(
            StatusOrdemServico.EmAnalise,
            Pagamentos: [new PagamentoDto(FormaPgto.Dinheiro, 999m)]);

        var resultado = await controller.AtualizarStatus(ordem.Id, dto);

        var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
        Log("PUT status EmAnalise com Pagamentos preenchido (deve ser ignorado)",
            esperado: StatusCodes.Status200OK, obtido: okResult.StatusCode);
        Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

        var pagamentosNoBanco = await context.Pagamentos.CountAsync(p => p.OrdemServicoId == ordem.Id);
        Log("Pagamentos persistidos após transição que não é Entregue", esperado: 0, obtido: pagamentosNoBanco);
        Assert.Equal(0, pagamentosNoBanco);
    }


}