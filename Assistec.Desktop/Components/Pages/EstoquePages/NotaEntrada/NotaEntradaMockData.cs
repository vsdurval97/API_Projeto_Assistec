namespace Assistec.Desktop.Components.Pages.EstoquePages.NotaEntrada;

public enum StatusNota { PendenteConferencia, EmConferencia, Conferida, EstoqueAtualizado, Cancelada }

public record ItemNotaEntradaLinha(
    string Codigo, string Descricao, string Ncm, string Cfop, string Unidade,
    double Quantidade, double ValorUnitario, string? UnidadeNota, double? QuantidadeNota, double? FatorConversao,
    double Desconto, double Icms, double Ipi, double Pis, double Cofins, double Ibs, double Cbs, double Is)
{
    public double Subtotal => Quantidade * ValorUnitario - Desconto;
}

public record NotaEntradaItem(
    string Id, string Numero, string Serie, string Chave, string Modelo,
    DateTime Emissao, DateTime Entrada, string Fornecedor, string Cnpj, string Ie,
    string? PedidoCompra, string Natureza, string TipoFrete, double ValorFrete, double OutrasDespesas,
    double Desconto, string CondicaoPagamento, string? Observacao, StatusNota Status,
    List<ItemNotaEntradaLinha> Itens)
{
    public double TotalProdutos => Itens.Sum(i => i.Subtotal);

    public double TotalNota => TotalProdutos + ValorFrete + OutrasDespesas - Desconto + Itens.Sum(i => i.Ipi + i.Is);

    public string StatusLabel => Status switch
    {
        StatusNota.PendenteConferencia => "Pendente de conferência",
        StatusNota.EmConferencia => "Em conferência",
        StatusNota.Conferida => "Conferida",
        StatusNota.EstoqueAtualizado => "Estoque atualizado",
        _ => "Cancelada"
    };

    private static string FormatarMoeda(double valor) => $"R$ {valor:N2}".Replace(",", "#").Replace(".", ",").Replace("#", ".");

    public string TotalNotaFormatado => FormatarMoeda(TotalNota);
}

public static class NotaEntradaMockData
{
    public static readonly List<NotaEntradaItem> Notas = new()
    {
        new("NE-00842", "000.018.492", "1", "35260912345678000190550010000184921000482176", "55 – NF-e",
            new DateTime(2026, 9, 9), new DateTime(2026, 9, 9), "Ferrotec Distribuição Ltda", "12.345.678/0001-90", "145.987.654.110",
            "PC-2026-0048", "Compra para comercialização", "CIF – Emitente", 0, 0,
            0, "28 dias", "Conferir lote e embalagem.", StatusNota.PendenteConferencia,
            new List<ItemNotaEntradaLinha>
            {
                new("PRD-001", "Parafuso Sextavado M8x50 Zincado", "73181590", "1102", "CX", 20, 18.90, null, null, null, 0, 45.36, 0, 2.46, 11.34, 0, 0, 0),
                new("PRD-004", "Cabo Elétrico Flexível 2,5mm² Azul", "85443000", "1102", "RL", 10, 142.00, null, null, null, 0, 170.40, 0, 9.23, 42.60, 0, 0, 0),
            }),

        new("NE-00841", "000.042.710", "2", "35260923456789000102550020000427101000193842", "55 – NF-e",
            new DateTime(2026, 9, 8), new DateTime(2026, 9, 9), "Distribuidora Norte Ltda", "23.456.789/0001-02", "267.111.458.009",
            "PC-2026-0047", "Compra para comercialização", "CIF – Emitente", 0, 0,
            125, "30/60 dias", null, StatusNota.EmConferencia,
            new List<ItemNotaEntradaLinha>
            {
                new("PRD-002", "Óleo Lubrificante ISO VG 46", "27101980", "1403", "UN", 25, 189.00, null, null, null, 125, 552.00, 0, 29.90, 138.00, 0, 0, 0),
            }),

        new("NE-00840", "000.003.188", "1", "35260934567890000114550010000031881000621954", "55 – NF-e",
            new DateTime(2026, 9, 7), new DateTime(2026, 9, 8), "EPI Sul Distribuidora Ltda", "34.567.890/0001-14", "904.188.331.2",
            "PC-2026-0046", "Compra para comercialização", "FOB – Destinatário", 95, 0,
            0, "21 dias", null, StatusNota.Conferida,
            new List<ItemNotaEntradaLinha>
            {
                new("PRD-005", "Luva de Segurança Látex CA 10005", "39261090", "1102", "PR", 60, 12.50, null, null, null, 0, 90.00, 0, 4.88, 22.50, 0, 0, 0),
            }),

        new("NE-00839", "000.091.206", "3", "35260945678901000126550030000912061000284710", "55 – NF-e",
            new DateTime(2026, 9, 5), new DateTime(2026, 9, 6), "Ferragens São Paulo ME", "45.678.901/0001-26", "118.727.901.118",
            null, "Compra para comercialização", "CIF – Emitente", 0, 18,
            0, "À vista", null, StatusNota.EstoqueAtualizado,
            new List<ItemNotaEntradaLinha>
            {
                new("PRD-003", "Chave de Fenda Phillips PH2 x 150mm", "82059000", "1102", "UN", 60, 8.40, null, null, null, 0, 60.48, 50.40, 3.28, 15.12, 0, 0, 0),
            }),

        new("NE-00838", "000.018.421", "1", "35260912345678000190550010000184211000370218", "55 – NF-e",
            new DateTime(2026, 9, 2), new DateTime(2026, 9, 3), "Ferrotec Distribuição Ltda", "12.345.678/0001-90", "145.987.654.110",
            "PC-2026-0044", "Compra para comercialização", "CIF – Emitente", 0, 0,
            0, "28 dias", null, StatusNota.EstoqueAtualizado,
            new List<ItemNotaEntradaLinha>
            {
                new("PRD-004", "Cabo Elétrico Flexível 2,5mm² Azul", "85443000", "1102", "RL", 8, 139.50, null, null, null, 0, 133.92, 0, 7.25, 33.48, 0, 0, 0),
            }),
    };
}