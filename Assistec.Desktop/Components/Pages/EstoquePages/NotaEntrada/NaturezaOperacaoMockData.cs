namespace Assistec.Desktop.Components.Pages.EstoquePages.NotaEntrada;

public record NaturezaOperacaoItem(string Nome, string Cfop, string Tipo, string Descricao);

public static class NaturezaOperacaoMockData
{
    public static readonly List<NaturezaOperacaoItem> Naturezas = new()
    {
        new("Compra para comercialização", "1102", "Entrada estadual", "Mercadoria adquirida dentro do estado para posterior revenda."),
        new("Compra para industrialização", "1101", "Entrada estadual", "Insumos adquiridos dentro do estado para utilização na produção."),
        new("Compra com substituição tributária", "1403", "Entrada estadual", "Mercadoria para comercialização sujeita ao regime de substituição tributária."),
        new("Compra interestadual para comercialização", "2102", "Entrada interestadual", "Mercadoria adquirida de fornecedor localizado em outro estado."),
        new("Devolução de venda", "1202", "Entrada estadual", "Retorno de mercadoria anteriormente vendida para comercialização."),
        new("Entrada em bonificação, doação ou brinde", "1910", "Outras entradas", "Entrada sem contraprestação financeira recebida do fornecedor."),
    };
}