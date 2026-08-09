// AssisTec.Tests/OrdemServicoTests.cs
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
}