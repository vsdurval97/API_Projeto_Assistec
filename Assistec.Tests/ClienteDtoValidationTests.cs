// AssisTec.Tests/ClienteDtoValidationTests.cs
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using AssistenciaTecnica.Api.Dtos;
using AssistenciaTecnica.Api.Models;
using FluentAssertions;
using Xunit;

namespace AssisTec.Tests;

// Testa a validação de DataAnnotations dos DTOs de Cliente isoladamente,
// sem controller nem HTTP. Usa o mesmo padrão de reflection já
// estabelecido em OrdemServicoControllerTests — reimplementado aqui
// (não movido para TesteBase) para manter esta etapa restrita a DTOs,
// sem tocar em arquivos de controller já estáveis.
//
// LIMITAÇÃO CONHECIDA: Validator.TryValidateObject não valida objetos
// aninhados (ex: EnderecoDto dentro de CriarClienteDto). Por isso,
// EnderecoDto é validado diretamente aqui, e a validação aninhada via
// payload real (CriarClienteDto.Endereco.Cep inválido, por exemplo) fica
// para o teste de integração HTTP, onde o pipeline real do ApiController
// valida objetos aninhados de fato.
public class ClienteDtoValidationTests
{
    private static bool EValido<T>(T modelo) where T : notnull
    {
        var tipo = typeof(T);
        var parametros = tipo.GetConstructors().First().GetParameters();

        foreach (var parametro in parametros)
        {
            var propriedade = tipo.GetProperty(parametro.Name!, BindingFlags.Public | BindingFlags.Instance);
            var valor = propriedade?.GetValue(modelo);

            var atributos = parametro.GetCustomAttributes(typeof(ValidationAttribute), inherit: true)
                .Cast<ValidationAttribute>();

            if (atributos.Any(atributo => !atributo.IsValid(valor)))
            {
                return false;
            }
        }

        return true;
    }

    // -------------------------------------------------------------------
    // EnderecoDto — validado isoladamente (ver nota de limitação acima)
    // -------------------------------------------------------------------

    [Theory(DisplayName = "EnderecoDto — CEP em formato válido (com ou sem hífen) deve passar na validação")]
    [InlineData("49040-490")]
    [InlineData("49040490")]
    [InlineData("49200-000")] // caso real: CEP único de Estância/SE
    public void EnderecoDto_CepValido_DeveSerValido(string cep)
        => EValido(new EnderecoDto(cep)).Should().BeTrue();

    [Theory(DisplayName = "EnderecoDto — CEP ausente, vazio ou em formato inválido deve falhar na validação")]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("abcde-123")]
    [InlineData("490404901")] // 9 dígitos
    public void EnderecoDto_CepInvalido_DeveSerInvalido(string cepInvalido)
        => EValido(new EnderecoDto(cepInvalido)).Should().BeFalse();

    [Fact(DisplayName = "EnderecoDto — Apenas Cep informado (restante nulo) deve ser válido")]
    public void EnderecoDto_ApenasCep_DeveSerValido()
        => EValido(new EnderecoDto("49040-490")).Should().BeTrue();

    // -------------------------------------------------------------------
    // CriarClienteDto — regressão dos campos originais + novos opcionais
    // -------------------------------------------------------------------

    [Fact(DisplayName = "CriarClienteDto — Apenas Nome e Telefone (sem nenhum dado fiscal) deve continuar válido")]
    public void CriarClienteDto_SemDadosFiscais_DeveSerValido()
    {
        // Confirma que a feature nova não tornou obrigatório nada que
        // antes não era — o cadastro rápido de balcão precisa continuar
        // funcionando exatamente como antes desta mudança.
        var dto = new CriarClienteDto("Ana Beatriz Costa", "79991234567");

        EValido(dto).Should().BeTrue();
    }

    [Fact(DisplayName = "CriarClienteDto — E-mail em formato inválido deve falhar na validação")]
    public void CriarClienteDto_EmailInvalido_DeveSerInvalido()
    {
        var dto = new CriarClienteDto("Ana Beatriz Costa", "79991234567", Email: "nao-e-um-email");

        EValido(dto).Should().BeFalse();
    }

    [Fact(DisplayName = "CriarClienteDto — E-mail nulo (não informado) deve ser válido")]
    public void CriarClienteDto_EmailNulo_DeveSerValido()
    {
        var dto = new CriarClienteDto("Ana Beatriz Costa", "79991234567", Email: null);

        EValido(dto).Should().BeTrue();
    }

    [Fact(DisplayName = "CriarClienteDto — TipoPessoa e IndicadorInscricaoEstadual devem assumir os defaults esperados quando omitidos")]
    public void CriarClienteDto_SemInformarEnumsOpcionais_DeveAssumirDefaultsCorretos()
    {
        var dto = new CriarClienteDto("Ana Beatriz Costa", "79991234567");

        dto.TipoPessoa.Should().Be(TipoPessoa.Fisica);
        dto.IndicadorInscricaoEstadual.Should().Be(IndicadorInscricaoEstadual.NaoContribuinte);
    }

    // -------------------------------------------------------------------
    // ClienteResponseDto.FromEntity — mapeamento correto, incluindo Endereco
    // -------------------------------------------------------------------

    [Fact(DisplayName = "ClienteResponseDto.FromEntity — Cliente sem Endereco deve mapear Endereco como null")]
    public void FromEntity_ClienteSemEndereco_DeveMapearEnderecoComoNull()
    {
        var cliente = new Cliente { Nome = "João", Telefone = "79999998888" };

        var dto = ClienteResponseDto.FromEntity(cliente);

        dto.Endereco.Should().BeNull();
    }

    [Fact(DisplayName = "ClienteResponseDto.FromEntity — Cliente com Endereco deve mapear todos os campos corretamente")]
    public void FromEntity_ClienteComEndereco_DeveMapearTodosOsCampos()
    {
        var cliente = new Cliente
        {
            Nome = "Maria",
            Telefone = "79988887777",
            Documento = "12345678900",
            Email = "maria@exemplo.com",
            Endereco = new Endereco
            {
                Cep = "49040490",
                Municipio = "Aracaju",
                Uf = "SE",
                CodigoMunicipioIbge = "2800308"
            }
        };

        var dto = ClienteResponseDto.FromEntity(cliente);

        dto.Documento.Should().Be("12345678900");
        dto.Email.Should().Be("maria@exemplo.com");
        dto.Endereco.Should().NotBeNull();
        dto.Endereco!.Municipio.Should().Be("Aracaju");
        dto.Endereco.CodigoMunicipioIbge.Should().Be("2800308");
    }
}