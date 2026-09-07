namespace Assistec.Desktop.Components.Pages.ParceirosPages.Fornecedores;

public enum StatusPedidoCompra { Recebido, EmTransito }

public record PedidoCompraItem(
    string FornecedorCodigo, string Numero, DateTime Data, string Itens,
    DateTime PrazoEntrega, string FormaPagamento, StatusPedidoCompra Status, double Valor)
{
    public string StatusLabel => Status == StatusPedidoCompra.Recebido ? "Recebido" : "Em trânsito";

    private static string FormatarMoeda(double valor) => $"R$ {valor:N2}".Replace(",", "#").Replace(".", ",").Replace("#", ".");

    public string ValorFormatado => FormatarMoeda(Valor);
}

public static class PedidoCompraMockData
{
    public static readonly List<PedidoCompraItem> Pedidos = new()
    {
        new("F0001", "PC-0112", new DateTime(2026, 8, 27), "Tela iPhone 13 Pro (5 un.), Bateria Sa...",
            new DateTime(2026, 8, 29), "30 ddl", StatusPedidoCompra.Recebido, 2850.00),

        new("F0001", "PC-0098", new DateTime(2026, 8, 10), "Conector USB-C (10 un.), Câmera tra...",
            new DateTime(2026, 8, 13), "30 ddl", StatusPedidoCompra.Recebido, 1640.00),

        new("F0001", "PC-0085", new DateTime(2026, 7, 18), "Kit ferramentas reparo iPhone (2 kits)",
            new DateTime(2026, 7, 22), "À vista", StatusPedidoCompra.Recebido, 480.00),

        new("F0001", "PC-0119", new DateTime(2026, 9, 1), "Tela Samsung A32 (4 un.), Bateria iP...",
            new DateTime(2026, 9, 3), "30 ddl", StatusPedidoCompra.EmTransito, 3200.00),
    };
}