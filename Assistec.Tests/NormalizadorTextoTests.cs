// AssisTec.Tests/NormalizadorTextoTests.cs
using AssistenciaTecnica.Api.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace AssisTec.Tests;

// Testa NormalizadorTexto isoladamente — sem AppDbContext, sem ILogger,
// sem controller. É uma função pura (texto -> texto), então não precisa
// de nenhuma infraestrutura além da própria entrada e saída.
public class NormalizadorTextoTests : TesteBase
{
    public NormalizadorTextoTests(ITestOutputHelper output) : base(output)
    {
    }

    [Theory(DisplayName = "RemoverAcentosEMinusculas — Deve tratar como iguais nomes com acentuação e caixa diferentes")]
    [InlineData("José da Costa", "jose da costa")]
    [InlineData("JOSÉ DA COSTA", "jose da costa")]
    [InlineData("José Da Costa", "jose da costa")]
    [InlineData("joSÉ dA cOsTa", "jose da costa")]
    public void RemoverAcentosEMinusculas_NomesEquivalentes_DeveGerarMesmoResultado(string entrada, string esperado)
    {
        // Act
        var resultado = NormalizadorTexto.RemoverAcentosEMinusculas(entrada);

        // Assert
        Log($"Normalização de '{entrada}'", esperado: esperado, obtido: resultado);
        Assert.Equal(esperado, resultado);
    }

    [Theory(DisplayName = "RemoverAcentosEMinusculas — Deve remover todos os tipos de acento comuns em nomes brasileiros")]
    [InlineData("André", "andre")]
    [InlineData("Ítalo", "italo")]
    [InlineData("Cláudia", "claudia")]
    [InlineData("Simões", "simoes")]
    [InlineData("Núñez", "nunez")]
    [InlineData("Ção", "cao")]
    public void RemoverAcentosEMinusculas_DiacriticosComuns_DeveRemoverCorretamente(string entrada, string esperado)
    {
        // Act
        var resultado = NormalizadorTexto.RemoverAcentosEMinusculas(entrada);

        // Assert
        Log($"Remoção de acento em '{entrada}'", esperado: esperado, obtido: resultado);
        Assert.Equal(esperado, resultado);
    }

    [Fact(DisplayName = "RemoverAcentosEMinusculas — Nomes visualmente diferentes não devem ser tratados como iguais")]
    public void RemoverAcentosEMinusculas_NomesDiferentes_NaoDeveGerarMesmoResultado()
    {
        // Arrange
        var resultado1 = NormalizadorTexto.RemoverAcentosEMinusculas("José da Costa");
        var resultado2 = NormalizadorTexto.RemoverAcentosEMinusculas("Joana da Costa");

        // Assert
        Log("Comparação entre 'José da Costa' e 'Joana da Costa'",
            esperado: "diferentes", obtido: resultado1 == resultado2 ? "iguais" : "diferentes");
        Assert.NotEqual(resultado1, resultado2);
    }

    [Fact(DisplayName = "RemoverAcentosEMinusculas — String vazia deve retornar string vazia")]
    public void RemoverAcentosEMinusculas_StringVazia_DeveRetornarVazia()
    {
        // Act
        var resultado = NormalizadorTexto.RemoverAcentosEMinusculas(string.Empty);

        // Assert
        Log("Normalização de string vazia", esperado: string.Empty, obtido: resultado);
        Assert.Equal(string.Empty, resultado);
    }

    [Theory(DisplayName = "RemoverAcentosEMinusculas — Texto sem acento ou já em minúsculas deve permanecer igual, só em lowercase")]
    [InlineData("joao", "joao")]
    [InlineData("MARIA", "maria")]
    [InlineData("Carlos", "carlos")]
    public void RemoverAcentosEMinusculas_TextoSemAcento_DeveApenasConverterParaMinusculas(string entrada, string esperado)
    {
        // Act
        var resultado = NormalizadorTexto.RemoverAcentosEMinusculas(entrada);

        // Assert
        Log($"Normalização de texto sem acento '{entrada}'", esperado: esperado, obtido: resultado);
        Assert.Equal(esperado, resultado);
    }
}