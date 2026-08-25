using AssistenciaTecnica.Api.Models;
using Xunit;
using Xunit.Abstractions;

namespace AssisTec.Tests;

// Testa a entidade OrdemServico diretamente, sem controller, sem
// AppDbContext e sem HTTP — em especial TryObterTransicoesPermitidas,
// que hoje só era exercitado indiretamente via OrdemServicoController.
// AtualizarStatus() em si já tem cobertura de ponta a ponta nos testes
// de controller (datas em UTC, salvaguarda de DataConclusao); aqui o
// foco é validar a máquina de estados isoladamente.
public class OrdemServicoTests : TesteBase
{
    public OrdemServicoTests(ITestOutputHelper output) : base(output)
    {
    }

    [Theory(DisplayName = "TryObterTransicoesPermitidas — Deve retornar exatamente as transições esperadas para cada status")]
    [MemberData(nameof(TransicoesEsperadas))]
    public void TryObterTransicoesPermitidas_StatusValido_DeveRetornarTransicoesCorretas(
        StatusOrdemServico origem, StatusOrdemServico[] transicoesEsperadas)
    {
        // Act
        var encontrou = OrdemServico.TryObterTransicoesPermitidas(origem, out var transicoesObtidas);

        // Assert
        Log($"TryObterTransicoesPermitidas para status '{origem}' — encontrado no mapa",
            esperado: true, obtido: encontrou);
        Assert.True(encontrou);

        Log($"Transições permitidas a partir de '{origem}'",
            esperado: string.Join(", ", transicoesEsperadas.Select(s => s.ToString())),
            obtido: string.Join(", ", transicoesObtidas.Select(s => s.ToString())));
        Assert.Equal(transicoesEsperadas, transicoesObtidas);
    }

    public static IEnumerable<object[]> TransicoesEsperadas()
    {
        yield return new object[] { StatusOrdemServico.Recebido, new[] { StatusOrdemServico.EmAnalise } };
        yield return new object[] { StatusOrdemServico.EmAnalise, new[] { StatusOrdemServico.Pronto, StatusOrdemServico.Recebido } };
        yield return new object[] { StatusOrdemServico.Pronto, new[] { StatusOrdemServico.Entregue, StatusOrdemServico.EmAnalise } };
        yield return new object[] { StatusOrdemServico.Entregue, Array.Empty<StatusOrdemServico>() };
    }

    [Fact(DisplayName = "TryObterTransicoesPermitidas — Status fora do mapa deve retornar false, sem lançar exceção")]
    public void TryObterTransicoesPermitidas_StatusForaDoMapa_DeveRetornarFalseSemLancarExcecao()
    {
        // Arrange — simula um valor de enum que não existe no dicionário
        // (dado corrompido no banco, ou enum estendido sem atualizar o mapa).
        var statusInvalido = (StatusOrdemServico)99;

        // Act
        var encontrou = OrdemServico.TryObterTransicoesPermitidas(statusInvalido, out var transicoes);

        // Assert
        Log("TryObterTransicoesPermitidas para status inexistente no mapa (99)",
            esperado: false, obtido: encontrou);
        Assert.False(encontrou);

        // Assert.NotNull primeiro, separado do Assert.Empty — evita qualquer
        // acesso a propriedade de um valor potencialmente nulo dentro do log.
        Log("Array de transições retornado para status inválido",
            esperado: "não nulo", obtido: transicoes is null ? "null" : "não nulo");
        Assert.NotNull(transicoes);

        Log("Quantidade de transições para status inválido",
            esperado: 0, obtido: transicoes.Length);
        Assert.Empty(transicoes);
    }

    [Theory(DisplayName = "TryObterTransicoesPermitidas — Entregue é estado final, sem nenhuma transição válida")]
    [InlineData(StatusOrdemServico.Recebido)]
    [InlineData(StatusOrdemServico.EmAnalise)]
    [InlineData(StatusOrdemServico.Pronto)]
    [InlineData(StatusOrdemServico.Entregue)]
    public void TryObterTransicoesPermitidas_Entregue_NuncaDeveAparecerComoOrigemComTransicoesValidas(
        StatusOrdemServico statusQualquer)
    {
        // Reforça, testando contra todo status possível, que nenhum deles
        // libera uma transição PARA fora de Entregue quando Entregue é a origem.
        OrdemServico.TryObterTransicoesPermitidas(StatusOrdemServico.Entregue, out var transicoesDeEntregue);

        Log($"'Entregue' permite transição para '{statusQualquer}'?",
            esperado: false, obtido: transicoesDeEntregue.Contains(statusQualquer));
        Assert.DoesNotContain(statusQualquer, transicoesDeEntregue);
    }

    // -------------------------------------------------------------------
    // TentarRegistrarPagamentos — isolado do controller e do HTTP, mesmo
    // espírito de TryObterTransicoesPermitidas: a regra de negócio ("a
    // soma dos pagamentos tem que bater com o ValorTotal") mora na
    // entidade, então é testada aqui sem precisar de AppDbContext.
    // -------------------------------------------------------------------

    private static OrdemServico CriarOrdemParaTeste(decimal valorMaoDeObra, decimal valorPecas) => new()
    {
        Marca = "Marca",
        Modelo = "Modelo",
        DefeitoRelatado = "Defeito qualquer, só para satisfazer o required",
        ValorMaoDeObra = valorMaoDeObra,
        ValorPecas = valorPecas
    };

    [Fact(DisplayName = "TentarRegistrarPagamentos — Um único pagamento cobrindo o valor total deve ser aceito")]
    public void TentarRegistrarPagamentos_UmPagamentoCobrindoOTotal_DeveRetornarTrueERegistrar()
    {
        var ordem = CriarOrdemParaTeste(valorMaoDeObra: 80m, valorPecas: 20m);
        var pagamentos = new List<Pagamento> { new() { Forma = FormaPgto.Dinheiro, Valor = 100m } };

        var aceito = ordem.TentarRegistrarPagamentos(pagamentos, out var erro);

        Log("TentarRegistrarPagamentos com pagamento único = ValorTotal (100)", esperado: true, obtido: aceito);
        Assert.True(aceito);

        Log("Mensagem de erro quando o registro é aceito", esperado: "null", obtido: erro ?? "null");
        Assert.Null(erro);

        Log("Quantidade de pagamentos registrados na OS", esperado: 1, obtido: ordem.Pagamentos.Count);
        Assert.Single(ordem.Pagamentos);
    }

    [Fact(DisplayName = "TentarRegistrarPagamentos — Múltiplas formas somando o valor total devem ser aceitas")]
    public void TentarRegistrarPagamentos_MultiplasFormasSomandoOTotal_DeveRetornarTrueERegistrarTodos()
    {
        var ordem = CriarOrdemParaTeste(valorMaoDeObra: 100m, valorPecas: 0m);
        var pagamentos = new List<Pagamento>
    {
        new() { Forma = FormaPgto.PagamentoInstantaneoPix, Valor = 60m },
        new() { Forma = FormaPgto.Dinheiro, Valor = 40m }
    };

        var aceito = ordem.TentarRegistrarPagamentos(pagamentos, out var erro);

        Log("TentarRegistrarPagamentos com Pix(60) + Dinheiro(40) = ValorTotal (100)",
            esperado: true, obtido: aceito);
        Assert.True(aceito);
        Assert.Null(erro);

        Log("Quantidade de pagamentos registrados na OS", esperado: 2, obtido: ordem.Pagamentos.Count);
        Assert.Equal(2, ordem.Pagamentos.Count);
    }

    [Fact(DisplayName = "TentarRegistrarPagamentos — Lista vazia deve ser rejeitada, sem alterar Pagamentos")]
    public void TentarRegistrarPagamentos_ListaVazia_DeveRetornarFalseSemAlterarPagamentos()
    {
        var ordem = CriarOrdemParaTeste(valorMaoDeObra: 50m, valorPecas: 0m);

        var aceito = ordem.TentarRegistrarPagamentos(Array.Empty<Pagamento>(), out var erro);

        Log("TentarRegistrarPagamentos com lista vazia", esperado: false, obtido: aceito);
        Assert.False(aceito);

        Log("Mensagem de erro para lista vazia",
            esperado: "não nula/vazia", obtido: string.IsNullOrWhiteSpace(erro) ? "nula/vazia" : "preenchida");
        Assert.False(string.IsNullOrWhiteSpace(erro));

        Log("Pagamentos da OS após tentativa rejeitada", esperado: 0, obtido: ordem.Pagamentos.Count);
        Assert.Empty(ordem.Pagamentos);
    }

    [Theory(DisplayName = "TentarRegistrarPagamentos — Soma divergente do ValorTotal (para menos ou para mais) deve ser rejeitada")]
    [InlineData(90.0)]  // menor que o total (100)
    [InlineData(150.0)] // maior que o total (100)
    public void TentarRegistrarPagamentos_SomaDivergenteDoTotal_DeveRetornarFalseSemAlterarPagamentos(double valorPago)
    {
        var ordem = CriarOrdemParaTeste(valorMaoDeObra: 70m, valorPecas: 30m); // ValorTotal = 100
        var pagamentos = new List<Pagamento> { new() { Forma = FormaPgto.CartaoCredito, Valor = (decimal)valorPago } };

        var aceito = ordem.TentarRegistrarPagamentos(pagamentos, out var erro);

        Log($"TentarRegistrarPagamentos com soma={valorPago} e ValorTotal=100", esperado: false, obtido: aceito);
        Assert.False(aceito);

        Log("Mensagem de erro para soma divergente",
            esperado: "não nula/vazia", obtido: string.IsNullOrWhiteSpace(erro) ? "nula/vazia" : "preenchida");
        Assert.False(string.IsNullOrWhiteSpace(erro));

        Log("Pagamentos da OS após tentativa rejeitada", esperado: 0, obtido: ordem.Pagamentos.Count);
        Assert.Empty(ordem.Pagamentos);
    }

}