using AssistenciaTecnica.Api.Dtos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AssistenciaTecnica.Api.Services;

public sealed class OrdemServicoPdfGenerator : IOrdemServicoPdfGenerator
{
    public byte[] Gerar(OrdemServicoPdfDto dados)
        // GeneratePdf() já retorna a partir de um MemoryStream interno do
        // QuestPDF — não há necessidade de tocar disco nem gerenciar um
        // Stream manualmente para produzir os bytes finais.
        => Document.Create(container => ComposeDocumento(container, dados)).GeneratePdf();

    private static void ComposeDocumento(IDocumentContainer container, OrdemServicoPdfDto dados)
{
    container.Page(page =>
    {
        page.Size(PageSizes.A4);
        page.Margin(40);
        page.DefaultTextStyle(estilo => estilo.FontSize(11));

        // Header()/Content()/Footer() retornam IContainer e não aceitam
        // lambda diretamente nesta versão do QuestPDF — .Element(...) é o
        // ponto de extensão correto para plugar um método de composição
        // customizado dentro de cada área da página.
        page.Header().Element(c => ComposeCabecalho(c, dados));
        page.Content().Element(c => ComposeConteudo(c, dados));
        page.Footer().Element(ComposeRodape);
    });
}

    private static void ComposeCabecalho(IContainer container, OrdemServicoPdfDto dados)
    {
        container.Column(coluna =>
        {
            coluna.Item().Text("Ordem de Serviço").FontSize(20).Bold();
            coluna.Item().Text($"Nº {dados.Id:D6}").FontSize(12);
            coluna.Item().PaddingTop(5).LineHorizontal(1);
        });
    }

    private static void ComposeConteudo(IContainer container, OrdemServicoPdfDto dados)
    {
        // Column, não linhas de altura fixa: um Height(x) fixo é a causa
        // mais comum de LayoutException no QuestPDF quando o conteúdo
        // (ex: DefeitoRelatado digitado livremente pelo técnico) excede o
        // espaço reservado. Column cresce conforme o conteúdo precisa.
        container.PaddingVertical(10).Column(coluna =>
        {
            coluna.Spacing(15);

            coluna.Item().Element(c => ComposeSecaoCliente(c, dados));
            coluna.Item().Element(c => ComposeSecaoEquipamento(c, dados));
            coluna.Item().Element(c => ComposeSecaoDefeito(c, dados));
            coluna.Item().Element(c => ComposeSecaoDatas(c, dados));
            coluna.Item().Element(c => ComposeSecaoValores(c, dados));
        });
    }

    private static void ComposeSecaoCliente(IContainer container, OrdemServicoPdfDto dados)
    {
        container.Column(coluna =>
        {
            coluna.Item().Text("Cliente").Bold().FontSize(13);
            coluna.Item().Text($"Nome: {dados.ClienteNome}");
            coluna.Item().Text($"Telefone: {dados.ClienteTelefoneFormatado}");
            coluna.Item().Text($"Documento: {dados.ClienteDocumentoFormatado}");
        });
    }

    private static void ComposeSecaoEquipamento(IContainer container, OrdemServicoPdfDto dados)
    {
        container.Column(coluna =>
        {
            coluna.Item().Text("Equipamento").Bold().FontSize(13);
            coluna.Item().Text($"Tipo: {dados.TipoEquipamento}");
            coluna.Item().Text($"Marca: {dados.Marca}");
            coluna.Item().Text($"Modelo: {dados.Modelo}");
            coluna.Item().Text($"Status: {dados.Status}");
        });
    }

    private static void ComposeSecaoDefeito(IContainer container, OrdemServicoPdfDto dados)
    {
        container.Column(coluna =>
        {
            coluna.Item().Text("Defeito Relatado").Bold().FontSize(13);
            // Sem limite de altura nem de caracteres deliberadamente: o
            // texto vem de um campo de digitação livre do técnico
            // (DefeitoRelatado aceita até 500 caracteres na validação de
            // entrada), e truncar aqui perderia informação do recibo.
            coluna.Item().Text(dados.DefeitoRelatado);
        });
    }

    private static void ComposeSecaoDatas(IContainer container, OrdemServicoPdfDto dados)
    {
        container.Row(linha =>
        {
            linha.RelativeItem().Text($"Abertura: {dados.DataAberturaFormatada}");
            linha.RelativeItem().Text($"Conclusão: {dados.DataConclusaoFormatada}");
            linha.RelativeItem().Text($"Entrega: {dados.DataEntregaFormatada}");
        });
    }

    private static void ComposeSecaoValores(IContainer container, OrdemServicoPdfDto dados)
    {
        container.Column(coluna =>
        {
            coluna.Item().Text("Valores").Bold().FontSize(13);
            coluna.Item().Text($"Mão de obra: {dados.ValorMaoDeObraFormatado}");
            coluna.Item().Text($"Peças: {dados.ValorPecasFormatado}");
            coluna.Item().PaddingTop(5).Text($"Total: {dados.ValorTotalFormatado}").Bold().FontSize(14);
        });
    }

    private static void ComposeRodape(IContainer container)
    {
        container.AlignCenter().Text(texto =>
        {
            texto.Span("Documento gerado eletronicamente em ");
            texto.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).Bold();
        });
    }
}