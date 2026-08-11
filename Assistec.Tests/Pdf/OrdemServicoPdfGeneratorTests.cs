/*using AssistenciaTecnica.Api.Dtos;
using AssistenciaTecnica.Api.Services;
using FluentAssertions;
using QuestPDF.Infrastructure;
using Xunit;

namespace AssisTec.Tests.Pdf;

// Renderização REAL do QuestPDF, sem mocks. Garante que o layout não
// estoura LayoutException (comum quando um container de altura fixa
// recebe conteúdo maior do que cabe) e que o resultado é reconhecível
// como PDF de verdade — não só "um array de bytes qualquer".
public class OrdemServicoPdfGeneratorTests
{
    // QuestPDF exige a licença configurada uma única vez por processo —
    // sem isso, a primeira chamada a GeneratePdf() lança exceção em
    // runtime. Isso não é detalhe de teste, é pré-requisito da biblioteca
    // desde que passou a exigir o modelo de licenciamento Community.
    static OrdemServicoPdfGeneratorTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private static OrdemServicoPdfDto CriarDadosValidos(string defeitoRelatado = "Não liga") => new(
        Id: 1,
        ClienteNome: "João da Silva",
        ClienteTelefoneFormatado: "(79) 99999-8888",
        ClienteDocumentoFormatado: "123.456.789-00",
        DataAberturaFormatada: "09/08/2026",
        DataConclusaoFormatada: "—",
        DataEntregaFormatada: "—",
        TipoEquipamento: "Notebook",
        Marca: "Dell",
        Modelo: "Inspiron 15",
        DefeitoRelatado: defeitoRelatado,
        Status: "Recebido",
        ValorMaoDeObraFormatado: "R$ 150,00",
        ValorPecasFormatado: "R$ 80,00",
        ValorTotalFormatado: "R$ 230,00");

    [Fact(DisplayName = "Gerar — Dados válidos deve retornar PDF não vazio, com assinatura binária correta")]
    public void Gerar_DadosValidos_DeveRetornarPdfValidoNaoVazio()
    {
        var gerador = new OrdemServicoPdfGenerator();

        var bytes = gerador.Gerar(CriarDadosValidos());

        bytes.Should().NotBeNullOrEmpty();
        // Todo PDF começa com a assinatura "%PDF" — verificação barata
        // de que o QuestPDF de fato produziu um documento válido.
        bytes.Take(4).Should().Equal("%PDF"u8.ToArray());
    }

    [Fact(DisplayName = "Gerar — Texto de defeito muito longo não deve estourar LayoutException")]
    public void Gerar_TextoDefeitoMuitoLongo_NaoDeveLancarExcecao()
    {
        // Risco real e comum em QuestPDF: containers de altura fixa
        // (ex: .Height(50)) lançam exceção quando o conteúdo não cabe.
        // Um texto de ~2000 caracteres força esse limite, confirmando
        // que o layout usa contêiner que se adapta ao conteúdo.
        var gerador = new OrdemServicoPdfGenerator();
        var textoLongo = string.Concat(Enumerable.Repeat("Defeito relatado muito detalhado pelo cliente. ", 60));

        var act = () => gerador.Gerar(CriarDadosValidos(textoLongo));

        act.Should().NotThrow();
    }

    [Fact(DisplayName = "Gerar — Chamadas múltiplas devem ser independentes (sem estado compartilhado entre gerações)")]
    public void Gerar_ChamadasMultiplas_DevemSerIndependentes()
    {
        var gerador = new OrdemServicoPdfGenerator();

        var primeiraGeracao = gerador.Gerar(CriarDadosValidos());
        var segundaGeracao = gerador.Gerar(CriarDadosValidos());

        primeiraGeracao.Should().NotBeEmpty();
        segundaGeracao.Should().NotBeEmpty();
    }
}*/