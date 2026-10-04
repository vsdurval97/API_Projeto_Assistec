namespace Assistec.Desktop.Components.Pages.EstoquePages.PedidoCompra;

public enum StatusPedidoDeCompra { Rascunho, AguardandoAprovacao, Aprovado, ParcialmenteRecebido, Concluido, Cancelado }

public record ItemPedidoCompraLinha(string Codigo, string Descricao, string Unidade, int Quantidade, double ValorUnitario)
{
    public double Subtotal => Quantidade * ValorUnitario;
}

public record PedidoCompraEstoqueItem(
    string Numero, string Fornecedor, string Cnpj, DateTime Emissao, DateTime Previsao,
    string Comprador, string Condicao, string Frete, string? Observacao,
    StatusPedidoDeCompra Status, List<ItemPedidoCompraLinha> Itens)
{
    public double Total => Itens.Sum(i => i.Subtotal);

    public string StatusLabel => Status switch
    {
        StatusPedidoDeCompra.Rascunho => "Rascunho",
        StatusPedidoDeCompra.AguardandoAprovacao => "Aguardando aprovação",
        StatusPedidoDeCompra.Aprovado => "Aprovado",
        StatusPedidoDeCompra.ParcialmenteRecebido => "Parcialmente recebido",
        StatusPedidoDeCompra.Concluido => "Concluído",
        _ => "Cancelado"
    };

    private static string FormatarMoeda(double valor) => $"R$ {valor:N2}".Replace(",", "#").Replace(".", ",").Replace("#", ".");

    public string TotalFormatado => FormatarMoeda(Total);
}

public static class PedidoCompraEstoqueMockData
{
    public static readonly List<PedidoCompraEstoqueItem> Pedidos = new()
    {
        new("PC-2026-0048", "Ferrotec Distribuição Ltda", "12.345.678/0001-90",
            new DateTime(2026, 9, 8), new DateTime(2026, 9, 15), "Mariana Costa", "28 dias", "CIF",
            "Priorizar itens com estoque crítico.", StatusPedidoDeCompra.AguardandoAprovacao,
            new List<ItemPedidoCompraLinha>
            {
                new("PRD-001", "Parafuso Sextavado M8x50 Zincado", "CX", 20, 18.90),
                new("PRD-004", "Cabo Elétrico Flexível 2,5mm² Azul", "RL", 10, 142.00),
            }),

        new("PC-2026-0047", "Distribuidora Norte Ltda", "23.456.789/0001-02",
            new DateTime(2026, 9, 7), new DateTime(2026, 9, 12), "Carlos Mendes", "30/60 dias", "CIF",
            null, StatusPedidoDeCompra.Aprovado,
            new List<ItemPedidoCompraLinha>
            {
                new("PRD-002", "Óleo Lubrificante ISO VG 46", "UN", 25, 189.00),
                new("PRD-006", "Rolamento Rígido de Esferas 6205-2RS", "UN", 40, 28.00),
            }),

        new("PC-2026-0046", "EPI Sul Distribuidora Ltda", "34.567.890/0001-14",
            new DateTime(2026, 9, 5), new DateTime(2026, 9, 10), "Mariana Costa", "21 dias", "FOB",
            "Entrega no almoxarifado 2.", StatusPedidoDeCompra.ParcialmenteRecebido,
            new List<ItemPedidoCompraLinha>
            {
                new("PRD-005", "Luva de Segurança Látex CA 10005", "PR", 100, 12.50),
            }),

        new("PC-2026-0045", "Ferragens São Paulo ME", "45.678.901/0001-26",
            new DateTime(2026, 9, 3), new DateTime(2026, 9, 9), "Rafael Lima", "À vista", "CIF",
            null, StatusPedidoDeCompra.Rascunho,
            new List<ItemPedidoCompraLinha>
            {
                new("PRD-003", "Chave de Fenda Phillips PH2 x 150mm", "UN", 60, 8.40),
            }),

        new("PC-2026-0044", "Ferrotec Distribuição Ltda", "12.345.678/0001-90",
            new DateTime(2026, 9, 1), new DateTime(2026, 9, 6), "Carlos Mendes", "28 dias", "CIF",
            null, StatusPedidoDeCompra.Concluido,
            new List<ItemPedidoCompraLinha>
            {
                new("PRD-004", "Cabo Elétrico Flexível 2,5mm² Azul", "RL", 8, 139.50),
            }),

        new("PC-2026-0043", "Metalúrgica Horizonte S.A.", "56.789.012/0001-38",
            new DateTime(2026, 8, 29), new DateTime(2026, 9, 5), "Rafael Lima", "30 dias", "FOB",
            null, StatusPedidoDeCompra.Cancelado,
            new List<ItemPedidoCompraLinha>
            {
                new("PRD-007", "Tubo de Aço Carbono Redondo 1\" Sch40", "UN", 30, 68.00),
            }),
    };
}