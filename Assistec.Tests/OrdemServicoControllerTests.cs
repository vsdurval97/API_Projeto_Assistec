// AssisTec.Tests/OrdemServicoControllerTests.cs
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using AssistenciaTecnica.Api.Controllers;
using AssistenciaTecnica.Api.Data;
using AssistenciaTecnica.Api.Dtos;
using AssistenciaTecnica.Api.Models;
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

public class OrdemServicoControllerTests
{
    private readonly ITestOutputHelper _output;

    public OrdemServicoControllerTests(ITestOutputHelper output)
    {
        _output = output;
    }

    // -----------------------------------------------------------------------
    // Helpers de infraestrutura
    // -----------------------------------------------------------------------

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

    // Loga de forma padronizada o que era esperado vs. o que foi obtido.
    // Chame ANTES do Assert, para o log aparecer mesmo se o teste falhar.
    private void Log(string cenario, object esperado, object? obtido)
    {
        _output.WriteLine($"CENÁRIO : {cenario}");
        _output.WriteLine($"ESPERADO: {esperado}");
        _output.WriteLine($"OBTIDO  : {obtido}");
        _output.WriteLine(new string('-', 60));
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

    // =========================================================================
    // 1. CRIAÇÃO — CENÁRIOS BÁSICOS
    // =========================================================================

    [Fact(DisplayName = "POST /ordemservico — Dados válidos deve retornar 201 Created com o DTO correto")]
    public async Task CriarOrdemServico_DadosValidos_DeveRetornar201ComDtoCorreto()
    {
        // Arrange
        await using var context = CriarContextoEmMemoria();
        var cliente = await CriarClienteAsync(context, "João da Silva");
        var controller = CriarController(context);

        var dto = new CriarOrdemServicoDto(
            TipoEquipamento.Notebook, "Dell", "Inspiron 15", "Não liga",
            150.00m, 80.00m, ClienteId: cliente.Id);

        // Act
        var resultado = await controller.CriarOrdemServico(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(resultado.Result);
        Log("Criar OS com ClienteId válido", esperado: StatusCodes.Status201Created, obtido: createdResult.StatusCode);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);

        var ordemCriada = Assert.IsType<OrdemServicoResponseDto>(createdResult.Value);
        Log("Status inicial da OS criada", esperado: StatusOrdemServico.Recebido, obtido: ordemCriada.Status);
        Assert.Equal(StatusOrdemServico.Recebido, ordemCriada.Status);

        var totalNoBanco = await context.OrdensServico.CountAsync();
        Log("Quantidade de OS persistidas no banco", esperado: 1, obtido: totalNoBanco);
        Assert.Equal(1, totalNoBanco);
    }

    [Fact(DisplayName = "POST /ordemservico — ClienteId inexistente deve retornar 404 NotFound")]
    public async Task CriarOrdemServico_ClienteIdInexistente_DeveRetornarNotFound()
    {
        // Arrange
        await using var context = CriarContextoEmMemoria();
        var controller = CriarController(context);

        var dto = new CriarOrdemServicoDto(
            TipoEquipamento.Impressora, "HP", "LaserJet", "Não imprime",
            50.00m, 0m, ClienteId: 999);

        // Act
        var resultado = await controller.CriarOrdemServico(dto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(resultado.Result);
        Log("Criar OS com ClienteId=999 (inexistente)",
            esperado: StatusCodes.Status404NotFound, obtido: notFoundResult.StatusCode);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);

        var totalNoBanco = await context.OrdensServico.CountAsync();
        Log("OS persistidas indevidamente", esperado: 0, obtido: totalNoBanco);
        Assert.Equal(0, totalNoBanco);
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
        var cliente = await CriarClienteAsync(context, "Maria Souza");
        var controller = CriarController(context);

        var dto = new CriarOrdemServicoDto(
            TipoEquipamento.Computador, "LG", "PC Gamer", "Tela azul",
            valorMaoDeObra, valorPecas, ClienteId: cliente.Id);

        // Act
        var resultado = await controller.CriarOrdemServico(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(resultado.Result);
        var ordemCriada = Assert.IsType<OrdemServicoResponseDto>(createdResult.Value);

        Log($"ValorTotal para MaoDeObra={valorMaoDeObra} + Pecas={valorPecas}",
            esperado: valorTotalEsperado, obtido: ordemCriada.ValorTotal);
        Assert.Equal(valorTotalEsperado, ordemCriada.ValorTotal);
    }

    // =========================================================================
    // 2. VALIDAÇÃO — VALORES NEGATIVOS E CAMPOS OBRIGATÓRIOS
    // =========================================================================

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
            TipoEquipamento.Computador, "Marca Teste", "Modelo Teste",
            "Defeito qualquer para fins de teste", valorMaoDeObra, valorPecas, ClienteId: 1);

        // Act
        bool modeloValido = ValidarModelo(dto, controller);

        // Assert
        Log($"Validação com ValorMaoDeObra={valorMaoDeObra}, ValorPecas={valorPecas}",
            esperado: "Inválido (False)", obtido: modeloValido ? "Válido (True)" : "Inválido (False)");
        Assert.False(modeloValido);
        Assert.False(controller.ModelState.IsValid);
    }

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
            TipoEquipamento.Notebook, marca!, modelo!, defeito!,
            100.00m, 50.00m, ClienteId: 1);

        // Act
        bool modeloValido = ValidarModelo(dto, controller);

        // Assert
        Log($"Validação com Marca='{marca}', Modelo='{modelo}', Defeito='{defeito}'",
            esperado: "Inválido (False)", obtido: modeloValido ? "Válido (True)" : "Inválido (False)");
        Assert.False(modeloValido);
        Assert.False(controller.ModelState.IsValid);
    }

    // =========================================================================
    // 3. BUSCA
    // =========================================================================

    [Fact(DisplayName = "GET /ordemservico/{id} — ID inexistente deve retornar 404 NotFound")]
    public async Task BuscarPorId_IdInexistente_DeveRetornarNotFound()
    {
        // Arrange
        await using var context = CriarContextoEmMemoria();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.BuscarPorId(9999);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(resultado.Result);
        Log("Buscar OS com Id=9999 (inexistente)",
            esperado: StatusCodes.Status404NotFound, obtido: notFoundResult.StatusCode);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    // =========================================================================
    // 4. MÁQUINA DE ESTADOS — ATUALIZAÇÃO DE STATUS
    // =========================================================================

    [Fact(DisplayName = "PUT /ordemservico/{id}/status — Alterar status de OS já Entregue deve retornar 400 BadRequest")]
    public async Task AtualizarStatus_OrdemJaEntregue_DeveRetornarBadRequest()
    {
        // Arrange
        await using var context = CriarContextoEmMemoria();
        var cliente = await CriarClienteAsync(context, "Carlos Pereira");

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
        ordem.AtualizarStatus(StatusOrdemServico.EmAnalise);
        ordem.AtualizarStatus(StatusOrdemServico.Pronto);
        ordem.AtualizarStatus(StatusOrdemServico.Entregue);

        context.OrdensServico.Add(ordem);
        await context.SaveChangesAsync();

        var controller = CriarController(context);
        var dto = new AtualizarStatusDto(StatusOrdemServico.EmAnalise);

        // Act
        var resultado = await controller.AtualizarStatus(ordem.Id, dto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado.Result);
        Log("Tentar mover OS 'Entregue' de volta para 'EmAnalise'",
            esperado: StatusCodes.Status400BadRequest, obtido: badRequestResult.StatusCode);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);

        var ordemNoBanco = await context.OrdensServico.AsNoTracking().FirstAsync(o => o.Id == ordem.Id);
        Log("Status da OS no banco após tentativa bloqueada",
            esperado: StatusOrdemServico.Entregue, obtido: ordemNoBanco.Status);
        Assert.Equal(StatusOrdemServico.Entregue, ordemNoBanco.Status);
    }

    [Fact(DisplayName = "PUT /ordemservico/{id}/status — Ao mudar para Pronto, DataConclusao deve ser preenchida com a data atual (UTC)")]
    public async Task AtualizarStatus_MudarParaPronto_DevePreencherDataConclusaoComDataAtual()
    {
        // Arrange
        await using var context = CriarContextoEmMemoria();
        var cliente = await CriarClienteAsync(context, "Fernanda Lima");

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
        await controller.AtualizarStatus(ordem.Id, new AtualizarStatusDto(StatusOrdemServico.EmAnalise));

        // A entidade agora grava em UTC (DateTime.UtcNow) — o teste precisa
        // comparar contra a mesma referência de tempo, não hora local.
        var antesDoAct = DateTime.UtcNow;

        // Act
        var resultado = await controller.AtualizarStatus(ordem.Id, new AtualizarStatusDto(StatusOrdemServico.Pronto));

        var depoisDoAct = DateTime.UtcNow;

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
        var ordemAtualizada = Assert.IsType<OrdemServicoResponseDto>(okResult.Value);

        Log("Status após transição EmAnalise -> Pronto", esperado: StatusOrdemServico.Pronto, obtido: ordemAtualizada.Status);
        Assert.Equal(StatusOrdemServico.Pronto, ordemAtualizada.Status);

        Log("DataConclusao esperada dentro do intervalo (UTC)",
            esperado: $"entre {antesDoAct:HH:mm:ss} e {depoisDoAct:HH:mm:ss} UTC",
            obtido: ordemAtualizada.DataConclusao?.ToString("HH:mm:ss") + " UTC (Kind=" + ordemAtualizada.DataConclusao?.Kind + ")");
        Assert.NotNull(ordemAtualizada.DataConclusao);
        Assert.InRange(ordemAtualizada.DataConclusao!.Value, antesDoAct.AddSeconds(-1), depoisDoAct.AddSeconds(5));
    }

    [Fact(DisplayName = "PUT /ordemservico/{id}/status — Ao mudar para Entregue, DataEntrega deve ser preenchida com a data atual (UTC)")]
    public async Task AtualizarStatus_MudarParaEntregue_DevePreencherDataEntregaComDataAtual()
    {
        // Arrange
        await using var context = CriarContextoEmMemoria();
        var cliente = await CriarClienteAsync(context, "Ricardo Santana");

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
        await controller.AtualizarStatus(ordem.Id, new AtualizarStatusDto(StatusOrdemServico.EmAnalise));
        await controller.AtualizarStatus(ordem.Id, new AtualizarStatusDto(StatusOrdemServico.Pronto));

        var antesDoAct = DateTime.UtcNow;

        // Act
        var resultado = await controller.AtualizarStatus(ordem.Id, new AtualizarStatusDto(StatusOrdemServico.Entregue));

        var depoisDoAct = DateTime.UtcNow;

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(resultado.Result);
        var ordemAtualizada = Assert.IsType<OrdemServicoResponseDto>(okResult.Value);

        Log("Status após transição Pronto -> Entregue", esperado: StatusOrdemServico.Entregue, obtido: ordemAtualizada.Status);
        Assert.Equal(StatusOrdemServico.Entregue, ordemAtualizada.Status);

        Log("DataEntrega preenchida (UTC)",
            esperado: "não nula",
            obtido: ordemAtualizada.DataEntrega?.ToString("HH:mm:ss") + " UTC (Kind=" + ordemAtualizada.DataEntrega?.Kind + ")");
        Assert.NotNull(ordemAtualizada.DataEntrega);
        Assert.NotNull(ordemAtualizada.DataConclusao);
        Assert.InRange(ordemAtualizada.DataEntrega!.Value, antesDoAct.AddSeconds(-1), depoisDoAct.AddSeconds(5));
    }

    [Fact(DisplayName = "OrdemServico.AtualizarStatus — Ir direto para Entregue sem DataConclusao prévia deve preenchê-la também (UTC)")]
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

        var antesDoAct = DateTime.UtcNow;

        // Act — chama o método de domínio diretamente (sem passar pelo EF Core,
        // então aqui NÃO há round-trip pelo banco — o valor já nasce com
        // Kind=Utc puro, vindo de DateTime.UtcNow dentro de AtualizarStatus).
        ordem.AtualizarStatus(StatusOrdemServico.Entregue);

        var depoisDoAct = DateTime.UtcNow;

        // Assert
        Log("DataConclusao preenchida via salvaguarda do domínio (UTC)",
            esperado: "não nula",
            obtido: ordem.DataConclusao?.ToString("HH:mm:ss") + " UTC");
        Assert.NotNull(ordem.DataConclusao);
        Assert.InRange(ordem.DataConclusao!.Value, antesDoAct.AddSeconds(-1), depoisDoAct.AddSeconds(5));

        Log("DataEntrega preenchida junto", esperado: "não nula", obtido: ordem.DataEntrega?.ToString("HH:mm:ss") + " UTC");
        Assert.NotNull(ordem.DataEntrega);
}

    // =========================================================================
    // 5. NOVO: RESOLUÇÃO DE CLIENTE POR NOME (ClienteId opcional / ClienteNome)
    // =========================================================================

    [Fact(DisplayName = "POST /ordemservico — Sem ClienteId e sem ClienteNome deve retornar 400 BadRequest")]
    public async Task CriarOrdemServico_SemClienteIdESemClienteNome_DeveRetornarBadRequest()
    {
        // Arrange
        await using var context = CriarContextoEmMemoria();
        var controller = CriarController(context);

        var dto = new CriarOrdemServicoDto(
            TipoEquipamento.Notebook, "Acer", "Aspire 5", "Tela quebrada",
            100m, 50m, ClienteId: null, ClienteNome: null);

        // Act
        var resultado = await controller.CriarOrdemServico(dto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado.Result);
        Log("Criar OS sem ClienteId e sem ClienteNome",
            esperado: StatusCodes.Status400BadRequest, obtido: badRequestResult.StatusCode);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact(DisplayName = "POST /ordemservico — Somente ClienteNome com um único resultado deve criar a OS com sucesso")]
    public async Task CriarOrdemServico_SomenteClienteNomeComUmResultado_DeveCriarComSucesso()
    {
        // Arrange
        await using var context = CriarContextoEmMemoria();
        var cliente = await CriarClienteAsync(context, "Patrícia Gomes");
        var controller = CriarController(context);

        var dto = new CriarOrdemServicoDto(
            TipoEquipamento.Computador, "Asus", "Vivobook", "Superaquecendo",
            70m, 0m, ClienteId: null, ClienteNome: "Patrícia Gomes");

        // Act
        var resultado = await controller.CriarOrdemServico(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(resultado.Result);
        Log("Criar OS somente com ClienteNome único",
            esperado: StatusCodes.Status201Created, obtido: createdResult.StatusCode);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);

        var ordemCriada = Assert.IsType<OrdemServicoResponseDto>(createdResult.Value);
        Log("ClienteId resolvido automaticamente pelo nome",
            esperado: cliente.Id, obtido: ordemCriada.ClienteId);
        Assert.Equal(cliente.Id, ordemCriada.ClienteId);
    }

    [Fact(DisplayName = "POST /ordemservico — ClienteNome inexistente deve retornar 404 NotFound")]
    public async Task CriarOrdemServico_ClienteNomeInexistente_DeveRetornarNotFound()
    {
        // Arrange
        await using var context = CriarContextoEmMemoria();
        var controller = CriarController(context);

        var dto = new CriarOrdemServicoDto(
            TipoEquipamento.Outros, "Marca X", "Modelo Y", "Defeito genérico",
            10m, 0m, ClienteId: null, ClienteNome: "Nome Que Não Existe");

        // Act
        var resultado = await controller.CriarOrdemServico(dto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(resultado.Result);
        Log("Criar OS com ClienteNome que não existe no banco",
            esperado: StatusCodes.Status404NotFound, obtido: notFoundResult.StatusCode);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact(DisplayName = "POST /ordemservico — ClienteNome com múltiplos resultados (concorrência) deve retornar 400 com lista de candidatos")]
    public async Task CriarOrdemServico_ClienteNomeComConcorrencia_DeveRetornarBadRequestComListaDeCandidatos()
    {
        // Arrange
        await using var context = CriarContextoEmMemoria();
        var cliente1 = await CriarClienteAsync(context, "José da Costa", "79911111111");
        var cliente2 = await CriarClienteAsync(context, "José da Costa", "79922222222");
        var controller = CriarController(context);

        var dto = new CriarOrdemServicoDto(
            TipoEquipamento.Impressora, "Brother", "DCP-T520W", "Toner vazando",
            40m, 15m, ClienteId: null, ClienteNome: "José da Costa");

        // Act
        var resultado = await controller.CriarOrdemServico(dto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(resultado.Result);
        Log("Criar OS com nome ambíguo (2 clientes 'José da Costa')",
            esperado: StatusCodes.Status400BadRequest, obtido: badRequestResult.StatusCode);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);

        // Confirma que nenhuma OS foi criada por engano durante a ambiguidade
        var totalNoBanco = await context.OrdensServico.CountAsync();
        Log("OS persistidas durante conflito de nomes", esperado: 0, obtido: totalNoBanco);
        Assert.Equal(0, totalNoBanco);
    }

    [Fact(DisplayName = "POST /ordemservico — ClienteId e ClienteNome informados juntos: ClienteId deve ter prioridade")]
    public async Task CriarOrdemServico_ClienteIdEClienteNomeInformados_ClienteIdDeveTerPrioridade()
    {
        // Arrange
        await using var context = CriarContextoEmMemoria();
        var clienteCorreto = await CriarClienteAsync(context, "Eduardo Nascimento");
        var clienteNomeIgnorado = await CriarClienteAsync(context, "Nome Que Deveria Ser Ignorado");
        var controller = CriarController(context);

        var dto = new CriarOrdemServicoDto(
            TipoEquipamento.Notebook, "Samsung", "Book X30", "Teclado não funciona",
            60m, 0m, ClienteId: clienteCorreto.Id, ClienteNome: clienteNomeIgnorado.Nome);

        // Act
        var resultado = await controller.CriarOrdemServico(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(resultado.Result);
        var ordemCriada = Assert.IsType<OrdemServicoResponseDto>(createdResult.Value);

        Log("ClienteId vinculado quando ambos ClienteId e ClienteNome são enviados",
            esperado: clienteCorreto.Id, obtido: ordemCriada.ClienteId);
        Assert.Equal(clienteCorreto.Id, ordemCriada.ClienteId);
    }
    // =========================================================================
    // 6. SERIALIZAÇÃO — TipoEquipamento aceito como STRING no JSON
    // =========================================================================

    [Theory(DisplayName = "POST /ordemservico — TipoEquipamento enviado como string deve ser desserializado corretamente")]
    [InlineData("Computador", TipoEquipamento.Computador)]
    [InlineData("Notebook", TipoEquipamento.Notebook)]
    [InlineData("Impressora", TipoEquipamento.Impressora)]
    [InlineData("Outros", TipoEquipamento.Outros)]
    public async Task CriarOrdemServico_TipoEquipamentoComoString_DeveDesserializarEEherdarCorretamente(
        string tipoEquipamentoJson, TipoEquipamento tipoEquipamentoEsperado)
    {
    // Arrange — simula exatamente o JSON que chegaria via HTTP no Swagger,
    // com o enum como texto em vez de número.
    var jsonRecebido = $$"""
        {
            "tipoEquipamento": "{{tipoEquipamentoJson}}",
            "marca": "Marca Teste",
            "modelo": "Modelo Teste",
            "defeitoRelatado": "Defeito de teste para validar serialização",
            "valorMaoDeObra": 50.00,
            "valorPecas": 10.00,
            "clienteId": null,
            "clienteNome": "Cliente Serialização"
        }
        """;

    var opcoesJson = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    // Act (parte 1) — desserializa o JSON exatamente como o ASP.NET faria
    // ao receber a requisição HTTP, usando o mesmo conversor configurado no Program.cs.
    var dto = JsonSerializer.Deserialize<CriarOrdemServicoDto>(jsonRecebido, opcoesJson);

    Log($"Desserialização de TipoEquipamento a partir da string '{tipoEquipamentoJson}'",
        esperado: tipoEquipamentoEsperado, obtido: dto?.TipoEquipamento.ToString() ?? "null");

    Assert.NotNull(dto);
    Assert.Equal(tipoEquipamentoEsperado, dto!.TipoEquipamento);

    // Act (parte 2) — confirma que o DTO desserializado também funciona
    // de ponta a ponta no controller, criando a OS normalmente.
    await using var context = CriarContextoEmMemoria();
    await CriarClienteAsync(context, "Cliente Serialização");
    var controller = CriarController(context);

    var resultado = await controller.CriarOrdemServico(dto);

    // Assert
    var createdResult = Assert.IsType<CreatedAtActionResult>(resultado.Result);
    var ordemCriada = Assert.IsType<OrdemServicoResponseDto>(createdResult.Value);

    Log("TipoEquipamento persistido na OS criada via string",
        esperado: tipoEquipamentoEsperado, obtido: ordemCriada.TipoEquipamento);
    Assert.Equal(tipoEquipamentoEsperado, ordemCriada.TipoEquipamento);
}

}