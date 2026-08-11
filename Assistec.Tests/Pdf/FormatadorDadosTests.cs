/*using AssistenciaTecnica.Api.Helpers;
using FluentAssertions;
using Xunit;

namespace AssisTec.Tests.Pdf;

public class FormatadorDadosTests
{
    [Theory(DisplayName = "FormatarCpfCnpj — 11 dígitos deve aplicar máscara de CPF")]
    [InlineData("12345678900", "123.456.789-00")]
    [InlineData("123.456.789-00", "123.456.789-00")] // já formatado: normaliza e reformata
    public void FormatarCpfCnpj_OnzeDigitos_DeveAplicarMascaraCpf(string entrada, string esperado)
        => FormatadorDados.FormatarCpfCnpj(entrada).Should().Be(esperado);

    [Theory(DisplayName = "FormatarCpfCnpj — 14 dígitos deve aplicar máscara de CNPJ")]
    [InlineData("12345678000199", "12.345.678/0001-99")]
    public void FormatarCpfCnpj_QuatorzeDigitos_DeveAplicarMascaraCnpj(string entrada, string esperado)
        => FormatadorDados.FormatarCpfCnpj(entrada).Should().Be(esperado);

    [Theory(DisplayName = "FormatarCpfCnpj — entrada nula, vazia ou com tamanho inválido nunca deve lançar exceção")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("123456789012345")]
    public void FormatarCpfCnpj_EntradaInvalida_DeveRetornarFallbackSemLancar(string? entrada)
    {
        // Documento é opcional no cadastro — o PDF precisa mostrar ALGO
        // (o próprio texto original ou um placeholder), nunca quebrar a
        // geração inteira por causa de um campo secundário mal preenchido.
        var act = () => FormatadorDados.FormatarCpfCnpj(entrada);

        act.Should().NotThrow();
        act().Should().NotBeNull();
    }

    [Theory(DisplayName = "FormatarTelefone — 11 dígitos (celular) deve incluir o 9º dígito na máscara")]
    [InlineData("79999998888", "(79) 99999-8888")]
    public void FormatarTelefone_OnzeDigitos_DeveAplicarMascaraCelular(string entrada, string esperado)
        => FormatadorDados.FormatarTelefone(entrada).Should().Be(esperado);

    [Theory(DisplayName = "FormatarTelefone — 10 dígitos (fixo) deve aplicar máscara sem o 9º dígito")]
    [InlineData("7933334444", "(79) 3333-4444")]
    public void FormatarTelefone_DezDigitos_DeveAplicarMascaraFixo(string entrada, string esperado)
        => FormatadorDados.FormatarTelefone(entrada).Should().Be(esperado);

    [Fact(DisplayName = "FormatarMoeda — Deve formatar no padrão monetário brasileiro")]
    public void FormatarMoeda_ValorPositivo_DeveFormatarEmPadraoBrasileiro()
        => FormatadorDados.FormatarMoeda(1234.5m).Should().Be("R$ 1.234,50");

    [Fact(DisplayName = "FormatarMoeda — Zero deve formatar como R$ 0,00, não como string vazia")]
    public void FormatarMoeda_Zero_DeveFormatarComoZeroExplicito()
        => FormatadorDados.FormatarMoeda(0m).Should().Be("R$ 0,00");

    [Fact(DisplayName = "FormatarData — Deve formatar no padrão dd/MM/yyyy")]
    public void FormatarData_DataValida_DeveFormatarNoPadraoBrasileiro()
    {
        var data = new DateTime(2026, 8, 9, 14, 30, 0, DateTimeKind.Utc);
        FormatadorDados.FormatarData(data).Should().Be("09/08/2026");
    }

    [Fact(DisplayName = "FormatarDataOpcional — Data nula deve retornar placeholder, não lançar exceção")]
    public void FormatarDataOpcional_DataNula_DeveRetornarPlaceholder()
        => FormatadorDados.FormatarDataOpcional(null).Should().Be("—");

    [Fact(DisplayName = "FormatarDataOpcional — Data preenchida deve formatar normalmente")]
    public void FormatarDataOpcional_DataPreenchida_DeveFormatarNormalmente()
        => FormatadorDados.FormatarDataOpcional(new DateTime(2026, 8, 9)).Should().Be("09/08/2026");
}*/