// AssisTec.Tests/ClienteEnderecoTests.cs
using AssistenciaTecnica.Api.Models;
using FluentAssertions;
using Xunit;

namespace AssisTec.Tests;

public class ClienteEnderecoTests
{
    [Fact(DisplayName = "Cliente — Endereco deve ser opcional (null) por padrão")]
    public void Cliente_SemEnderecoInformado_DeveAceitarNull()
    {
        var cliente = new Cliente { Nome = "João", Telefone = "79999998888" };

        cliente.Endereco.Should().BeNull();
    }

    [Fact(DisplayName = "Cliente — TipoPessoa deve ter Fisica como padrão")]
    public void Cliente_SemTipoPessoaInformado_DeveTerFisicaComoPadrao()
    {
        var cliente = new Cliente { Nome = "João", Telefone = "79999998888" };

        cliente.TipoPessoa.Should().Be(TipoPessoa.Fisica);
    }

    [Fact(DisplayName = "Cliente — IndicadorInscricaoEstadual deve ter NaoContribuinte como padrão")]
    public void Cliente_SemIndicadorInformado_DeveTerNaoContribuinteComoPadrao()
    {
        var cliente = new Cliente { Nome = "João", Telefone = "79999998888" };

        cliente.IndicadorInscricaoEstadual.Should().Be(IndicadorInscricaoEstadual.NaoContribuinte);
    }

    [Fact(DisplayName = "Cliente — Documento, Email e InscricaoEstadual devem ser opcionais (null) por padrão")]
    public void Cliente_CamposFiscaisOpcionais_DevemAceitarNullPorPadrao()
    {
        var cliente = new Cliente { Nome = "João", Telefone = "79999998888" };

        cliente.Documento.Should().BeNull();
        cliente.Email.Should().BeNull();
        cliente.InscricaoEstadual.Should().BeNull();
    }

    [Fact(DisplayName = "Cliente — Endereco preenchido deve aceitar apenas Cep, com o restante opcional")]
    public void Cliente_EnderecoComApenasCep_DeveAceitarRestanteComoNulo()
    {
        var cliente = new Cliente
        {
            Nome = "João",
            Telefone = "79999998888",
            Endereco = new Endereco { Cep = "49200000" }
        };

        cliente.Endereco.Should().NotBeNull();
        cliente.Endereco!.Cep.Should().Be("49200000");
        cliente.Endereco.Logradouro.Should().BeNull();
        cliente.Endereco.Numero.Should().BeNull();
        cliente.Endereco.Bairro.Should().BeNull();
        cliente.Endereco.Municipio.Should().BeNull();
        cliente.Endereco.Uf.Should().BeNull();
        cliente.Endereco.CodigoMunicipioIbge.Should().BeNull();
    }

    [Fact(DisplayName = "Endereco — CodigoPais e Pais devem ter valores padrão Brasil (1058)")]
    public void Endereco_SemCodigoPaisInformado_DeveTerBrasilComoPadrao()
    {
        var endereco = new Endereco { Cep = "49200000" };

        endereco.CodigoPais.Should().Be("1058");
        endereco.Pais.Should().Be("Brasil");
    }

    [Fact(DisplayName = "Endereco — Deve aceitar todos os campos preenchidos (caso de CEP granular, cidade grande)")]
    public void Endereco_TodosOsCamposPreenchidos_DeveAceitarSemRestricao()
    {
        var endereco = new Endereco
        {
            Cep = "49040490",
            Logradouro = "Rua Simeão Sobral",
            Numero = "123",
            Complemento = "Apto 4",
            Bairro = "Suíssa",
            Municipio = "Aracaju",
            Uf = "SE",
            CodigoMunicipioIbge = "2800308"
        };

        endereco.Logradouro.Should().Be("Rua Simeão Sobral");
        endereco.Numero.Should().Be("123");
        endereco.Bairro.Should().Be("Suíssa");
        endereco.Municipio.Should().Be("Aracaju");
        endereco.CodigoMunicipioIbge.Should().Be("2800308");
    }

    [Fact(DisplayName = "Endereco — Deve representar corretamente o caso de CEP genérico (Estância/SE), com Logradouro e Bairro vazios")]
    public void Endereco_CepGenericoSemLogradouroEBairro_DeveAceitarComoValido()
    {
        // Não é um estado inválido — é o comportamento esperado para
        // municípios com CEP único cobrindo toda a área urbana.
        var endereco = new Endereco
        {
            Cep = "49200000",
            Municipio = "Estância",
            Uf = "SE",
            CodigoMunicipioIbge = "2802908",
            Logradouro = null,
            Bairro = null
        };

        endereco.Municipio.Should().Be("Estância");
        endereco.Logradouro.Should().BeNull();
        endereco.Bairro.Should().BeNull();
    }
}