/*using AssistenciaTecnica.Api.Dtos;
using AssistenciaTecnica.Api.Models;
using FluentAssertions;
using Xunit;

namespace AssisTec.Tests.Pdf;

public class OrdemServicoPdfDtoTests
{
    private static OrdemServico CriarOrdemValida(decimal maoDeObra = 100m, decimal pecas = 50m) => new()
    {
        Id = 1,
        TipoEquipamento = TipoEquipamento.Notebook,
        Marca = "Dell",
        Modelo = "Inspiron 15",
        DefeitoRelatado = "Não liga",
        ValorMaoDeObra = maoDeObra,
        ValorPecas = pecas,
        ClienteId = 1,
        Cliente = new Cliente { Id = 1, Nome = "João da Silva", Telefone = "79999998888", Documento = "12345678900" }
    };

    [Fact(DisplayName = "FromEntity — ValorTotal formatado deve ser a soma exata de mão de obra e peças")]
    public void FromEntity_ValoresValidos_ValorTotalDeveSerSomaCorreta()
    {
        var dto = OrdemServicoPdfDto.FromEntity(CriarOrdemValida(maoDeObra: 150m, pecas: 80m));

        dto.ValorTotalFormatado.Should().Be("R$ 230,00");
    }

    [Fact(DisplayName = "FromEntity — Deve propagar dados do cliente já formatados para exibição")]
    public void FromEntity_ClientePreenchido_DevePropagarDadosFormatados()
    {
        var dto = OrdemServicoPdfDto.FromEntity(CriarOrdemValida());

        dto.ClienteNome.Should().Be("João da Silva");
        dto.ClienteTelefoneFormatado.Should().Be("(79) 99999-8888");
        dto.ClienteDocumentoFormatado.Should().Be("123.456.789-00");
    }

    [Fact(DisplayName = "FromEntity — Cliente nulo nunca deve gerar PDF sem cliente; deve lançar exceção")]
    public void FromEntity_ClienteNulo_DeveLancarArgumentNullException()
    {
        var ordem = CriarOrdemValida();
        ordem.Cliente = null;

        var act = () => OrdemServicoPdfDto.FromEntity(ordem);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory(DisplayName = "FromEntity — Valores negativos (dado corrompido) devem bloquear a geração, não só logar")]
    [InlineData(-10, 0)]
    [InlineData(0, -5)]
    public void FromEntity_ValoresNegativos_DeveLancarInvalidOperationException(decimal maoDeObra, decimal pecas)
    {
        // Defesa em profundidade: os DTOs de entrada já bloqueiam valor
        // negativo na criação da OS, mas essa camada não deveria CONFIAR
        // cegamente nisso — um dado corrompido no banco não deve virar
        // um recibo com valor negativo impresso.
        var act = () => OrdemServicoPdfDto.FromEntity(CriarOrdemValida(maoDeObra, pecas));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact(DisplayName = "FromEntity — Datas de conclusão/entrega nulas geram placeholder, sem lançar exceção")]
    public void FromEntity_DatasOpcionaisNulas_DeveGerarPlaceholderSemErro()
    {
        var dto = OrdemServicoPdfDto.FromEntity(CriarOrdemValida()); // OS recém-criada: sem conclusão/entrega

        dto.DataConclusaoFormatada.Should().Be("—");
        dto.DataEntregaFormatada.Should().Be("—");
    }
}*/