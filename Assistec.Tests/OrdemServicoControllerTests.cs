
using AssistenciaTecnica.Api.Controllers;
using AssistenciaTecnica.Api.Data;
using AssistenciaTecnica.Api.Dtos;
using AssistenciaTecnica.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using NSubstitute;
using Xunit;

namespace AssisTec.Tests;

public class OrdemServicoControllerTests
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

    private static OrdemServicoController CriarController(AppDbContext context)
    {
        var loggerFalso = Substitute.For<ILogger<OrdemServicoController>>();
        return new OrdemServicoController(context, loggerFalso);
    }

    [Fact(DisplayName = "POST /ordemservico — Dados válidos deve retornar 201 Created com o DTO correto")]
    public async Task CriarOrdemServico_DadosValidos_DeveRetornar201ComDtoCorreto()
    {
        // Arrange
        await using var context = CriarContextoEmMemoria();

        var cliente = new Cliente { Nome = "João da Silva", Telefone = "79999998888" };
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        var controller = CriarController(context);

        var dto = new CriarOrdemServicoDto(
            TipoEquipamento.Notebook,
            "Dell",
            "Inspiron 15",
            "Não liga",
            150.00m,
            80.00m,
            cliente.Id
        );

        // Act
        var resultado = await controller.CriarOrdemServico(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(resultado.Result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);

        var ordemCriada = Assert.IsType<OrdemServicoResponseDto>(createdResult.Value);
        Assert.Equal(dto.Marca, ordemCriada.Marca);
        Assert.Equal(dto.Modelo, ordemCriada.Modelo);
        Assert.Equal(dto.ClienteId, ordemCriada.ClienteId);
        Assert.Equal(StatusOrdemServico.Recebido, ordemCriada.Status);

        // Confirma que a persistência realmente ocorreu no banco
        var totalNoBanco = await context.OrdensServico.CountAsync();
        Assert.Equal(1, totalNoBanco);
    }

    [Fact(DisplayName = "POST /ordemservico — ClienteId inexistente deve retornar 404 NotFound")]
    public async Task CriarOrdemServico_ClienteIdInexistente_DeveRetornarNotFound()
    {
        // Arrange
        await using var context = CriarContextoEmMemoria(); // banco vazio, nenhum cliente cadastrado
        var controller = CriarController(context);

        var dto = new CriarOrdemServicoDto(
            TipoEquipamento.Impressora,
            "HP",
            "LaserJet",
            "Não imprime",
            50.00m,
            0m,
            ClienteId: 999 // não existe no banco
        );

        // Act
        var resultado = await controller.CriarOrdemServico(dto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(resultado.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);

        // Garante que nada foi persistido indevidamente
        var nenhumaOrdemPersistida = await context.OrdensServico.CountAsync();
        Assert.Equal(0, nenhumaOrdemPersistida);
    }

    [Theory(DisplayName = "POST /ordemservico — ValorTotal deve ser exatamente MaoDeObra + Pecas")]
    [InlineData(150.00, 80.00, 230.00)]
    [InlineData(0, 0, 0)]
    [InlineData(999.99, 0.01, 1000.00)]
    public async Task CriarOrdemServico_ValorTotal_DeveSerSomaExataDeMaoDeObraEPecas(
        decimal valorMaoDeObra, decimal valorPecas, decimal valorTotalEsperado)
    {
        // Arrange
        await using var context = CriarContextoEmMemoria();

        var cliente = new Cliente { Nome = "Maria Souza", Telefone = "79988887777" };
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();

        var controller = CriarController(context);

        var dto = new CriarOrdemServicoDto(
            TipoEquipamento.Computador,
            "LG",
            "PC Gamer",
            "Tela azul",
            valorMaoDeObra,
            valorPecas,
            cliente.Id
        );

        // Act
        var resultado = await controller.CriarOrdemServico(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(resultado.Result);
        var ordemCriada = Assert.IsType<OrdemServicoResponseDto>(createdResult.Value);

        Assert.Equal(valorTotalEsperado, ordemCriada.ValorTotal);
    }
    // -----------------------------------------------------------------------
// Helper: simula a validação automática de ModelState que o [ApiController]
// faz no pipeline real, mas que não roda quando a action é chamada direto
// (como acontece em um teste unitário).
// -----------------------------------------------------------------------
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

// -----------------------------------------------------------------------
// 1. Mão de Obra ou Peças Negativas
// -----------------------------------------------------------------------
[Theory(DisplayName = "POST /ordemservico — Valores negativos devem falhar na validação de ModelState")]
[InlineData(-50.00, 0)]
[InlineData(0, -10.00)]
[InlineData(-1, -1)]
public void CriarOrdemServico_ValoresNegativos_DeveFalharValidacao(decimal valorMaoDeObra, decimal valorPecas)
{
    // Arrange
    using var context = CriarContextoEmMemoria();
    var controller = CriarController(context);

    var dto = new CriarOrdemServicoDto(
        TipoEquipamento.Computador,
        "Marca Teste",
        "Modelo Teste",
        "Defeito qualquer para fins de teste",
        valorMaoDeObra,
        valorPecas,
        ClienteId: 1
    );

    // Act
    bool modeloValido = ValidarModelo(dto, controller);

    // Assert
    Assert.False(modeloValido);
    Assert.False(controller.ModelState.IsValid);
}

// -----------------------------------------------------------------------
// 2. Buscar ID Inexistente
// -----------------------------------------------------------------------
[Fact(DisplayName = "GET /ordemservico/{id} — ID inexistente deve retornar 404 NotFound")]
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

// -----------------------------------------------------------------------
// 3. Campos Obrigatórios Vazios ou Nulos
// -----------------------------------------------------------------------
[Theory(DisplayName = "POST /ordemservico — Marca, Modelo ou Defeito vazios/nulos devem falhar na validação")]
[InlineData("", "Modelo válido", "Defeito relatado válido")]
[InlineData("Marca válida", "", "Defeito relatado válido")]
[InlineData("Marca válida", "Modelo válido", "")]
[InlineData(null, "Modelo válido", "Defeito relatado válido")]
[InlineData("Marca válida", null, "Defeito relatado válido")]
[InlineData("Marca válida", "Modelo válido", null)]
public void CriarOrdemServico_CamposObrigatoriosVaziosOuNulos_DeveFalharValidacao(
    string? marca, string? modelo, string? defeito)
{
    // Arrange
    using var context = CriarContextoEmMemoria();
    var controller = CriarController(context);

    var dto = new CriarOrdemServicoDto(
        TipoEquipamento.Notebook,
        marca!,
        modelo!,
        defeito!,
        100.00m,
        50.00m,
        ClienteId: 1
    );

    // Act
    bool modeloValido = ValidarModelo(dto, controller);

    // Assert
    Assert.False(modeloValido);
    Assert.False(controller.ModelState.IsValid);
}

// -----------------------------------------------------------------------
// 4. Fluxo de Status — bloquear alteração de OS já Entregue
// -----------------------------------------------------------------------
[Fact(DisplayName = "PUT /ordemservico/{id}/status — Alterar status de OS já Entregue deve retornar 400 BadRequest")]
public async Task AtualizarStatus_OrdemJaEntregue_DeveRetornarBadRequest()
{
    // Arrange
    await using var context = CriarContextoEmMemoria();

    var cliente = new Cliente { Nome = "Carlos Pereira", Telefone = "79977776666" };
    context.Clientes.Add(cliente);
    await context.SaveChangesAsync();

    var ordem = new OrdemServico
    {
        TipoEquipamento = TipoEquipamento.Impressora,
        Marca = "Epson",
        Modelo = "L3250",
        DefeitoRelatado = "Não puxa papel",
        ValorMaoDeObra = 80.00m,
        ValorPecas = 20.00m,
        ClienteId = cliente.Id
    };

    // Percorre o fluxo completo até "Entregue" usando o método de domínio
    ordem.AtualizarStatus(StatusOrdemServico.EmAnalise);
    ordem.AtualizarStatus(StatusOrdemServico.Pronto);
    ordem.AtualizarStatus(StatusOrdemServico.Entregue);

    context.OrdensServico.Add(ordem);
    await context.SaveChangesAsync();

    var controller = CriarController(context);

    // Tenta "regredir" a ordem já entregue de volta para EmAnalise
    var dto = new AtualizarStatusDto(StatusOrdemServico.EmAnalise);

    // Act
    var resultado = await controller.AtualizarStatus(ordem.Id, dto);

    // Assert
    var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado.Result);
    Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);

    // Garante que o status no banco não foi alterado
    var ordemNoBanco = await context.OrdensServico.AsNoTracking().FirstAsync(o => o.Id == ordem.Id);
    Assert.Equal(StatusOrdemServico.Entregue, ordemNoBanco.Status);
}

// -----------------------------------------------------------------------
// 5. Mudança para Pronto — DataConclusao deve ser preenchida com a data atual
// -----------------------------------------------------------------------
[Fact(DisplayName = "PUT /ordemservico/{id}/status — Ao mudar para Pronto, DataConclusao deve ser preenchida com a data atual")]
public async Task AtualizarStatus_MudarParaPronto_DevePreencherDataConclusaoComDataAtual()
{
    // Arrange
    await using var context = CriarContextoEmMemoria();

    var cliente = new Cliente { Nome = "Fernanda Lima", Telefone = "79966665555" };
    context.Clientes.Add(cliente);
    await context.SaveChangesAsync();

    var ordem = new OrdemServico
    {
        TipoEquipamento = TipoEquipamento.Computador,
        Marca = "Positivo",
        Modelo = "Master N",
        DefeitoRelatado = "Não inicializa o sistema",
        ValorMaoDeObra = 120.00m,
        ValorPecas = 0m,
        ClienteId = cliente.Id
    };

    context.OrdensServico.Add(ordem);
    await context.SaveChangesAsync();

    var controller = CriarController(context);

    // Fluxo válido conforme a máquina de estados do controller:
    // Recebido -> EmAnalise -> Pronto
    await controller.AtualizarStatus(ordem.Id, new AtualizarStatusDto(StatusOrdemServico.EmAnalise));

    var antesDoAct = DateTime.Now;

    // Act
    var resultado = await controller.AtualizarStatus(ordem.Id, new AtualizarStatusDto(StatusOrdemServico.Pronto));

    var depoisDoAct = DateTime.Now;

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
    var ordemAtualizada = Assert.IsType<OrdemServicoResponseDto>(okResult.Value);

    Assert.Equal(StatusOrdemServico.Pronto, ordemAtualizada.Status);
    Assert.NotNull(ordemAtualizada.DataConclusao);

    // Verifica que DataConclusao está dentro da janela de execução do teste,
    // evitando flakiness por diferença de milissegundos entre DateTime.Now e a asserção.
    Assert.InRange(ordemAtualizada.DataConclusao!.Value, antesDoAct.AddSeconds(-1), depoisDoAct.AddSeconds(5));
}

// -----------------------------------------------------------------------
// 6. Mudança para Entregue — DataEntrega deve ser preenchida com a data atual
// -----------------------------------------------------------------------
[Fact(DisplayName = "PUT /ordemservico/{id}/status — Ao mudar para Entregue, DataEntrega deve ser preenchida com a data atual")]
public async Task AtualizarStatus_MudarParaEntregue_DevePreencherDataEntregaComDataAtual()
{
    // Arrange
    await using var context = CriarContextoEmMemoria();

    var cliente = new Cliente { Nome = "Ricardo Santana", Telefone = "79955554444" };
    context.Clientes.Add(cliente);
    await context.SaveChangesAsync();

    var ordem = new OrdemServico
    {
        TipoEquipamento = TipoEquipamento.Impressora,
        Marca = "Canon",
        Modelo = "G3111",
        DefeitoRelatado = "Cabeça de impressão entupida",
        ValorMaoDeObra = 90.00m,
        ValorPecas = 35.00m,
        ClienteId = cliente.Id
    };

    context.OrdensServico.Add(ordem);
    await context.SaveChangesAsync();

    var controller = CriarController(context);

    // Fluxo válido completo: Recebido -> EmAnalise -> Pronto -> Entregue
    await controller.AtualizarStatus(ordem.Id, new AtualizarStatusDto(StatusOrdemServico.EmAnalise));
    await controller.AtualizarStatus(ordem.Id, new AtualizarStatusDto(StatusOrdemServico.Pronto));

    var antesDoAct = DateTime.Now;

    // Act
    var resultado = await controller.AtualizarStatus(ordem.Id, new AtualizarStatusDto(StatusOrdemServico.Entregue));

    var depoisDoAct = DateTime.Now;

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
    var ordemAtualizada = Assert.IsType<OrdemServicoResponseDto>(okResult.Value);

    Assert.Equal(StatusOrdemServico.Entregue, ordemAtualizada.Status);
    Assert.NotNull(ordemAtualizada.DataEntrega);
    Assert.NotNull(ordemAtualizada.DataConclusao); // já preenchida na etapa "Pronto"

    Assert.InRange(ordemAtualizada.DataEntrega!.Value, antesDoAct.AddSeconds(-1), depoisDoAct.AddSeconds(5));
}

// -----------------------------------------------------------------------
// 7. [Teste de unidade na entidade] Entregue sem passar por Pronto —
// garante que a salvaguarda do domínio preenche DataConclusao mesmo assim.
// Este cenário não é alcançável via PUT do controller (bloqueado pela
// máquina de estados), então é testado diretamente contra a entidade.
// -----------------------------------------------------------------------
[Fact(DisplayName = "OrdemServico.AtualizarStatus — Ir direto para Entregue sem DataConclusao prévia deve preenchê-la também")]
public void AtualizarStatus_EntidadeDireitoParaEntregueSemDataConclusaoPrevia_DevePreencherAmbasAsDatas()
{
    // Arrange
    var ordem = new OrdemServico
    {
        TipoEquipamento = TipoEquipamento.Outros,
        Marca = "Genérica",
        Modelo = "Teste Unitário",
        DefeitoRelatado = "Cenário de borda para validar salvaguarda do domínio",
        ValorMaoDeObra = 0m,
        ValorPecas = 0m,
        ClienteId = 1
    };

    Assert.Null(ordem.DataConclusao); // pré-condição: nunca passou por "Pronto"

    var antesDoAct = DateTime.Now;

    // Act — chama o método de domínio diretamente, pulando o controller
    // e a máquina de estados dele, para exercitar a salvaguarda interna.
    ordem.AtualizarStatus(StatusOrdemServico.Entregue);

    var depoisDoAct = DateTime.Now;

    // Assert
    Assert.Equal(StatusOrdemServico.Entregue, ordem.Status);
    Assert.NotNull(ordem.DataConclusao);
    Assert.NotNull(ordem.DataEntrega);

    Assert.InRange(ordem.DataConclusao!.Value, antesDoAct.AddSeconds(-1), depoisDoAct.AddSeconds(5));
    Assert.InRange(ordem.DataEntrega!.Value, antesDoAct.AddSeconds(-1), depoisDoAct.AddSeconds(5));
}

}