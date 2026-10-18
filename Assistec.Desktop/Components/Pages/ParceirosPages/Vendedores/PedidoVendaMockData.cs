namespace Assistec.Desktop.Components.Pages.ParceirosPages.Vendedores;

public enum StatusPedidoVenda { Faturado, Pendente, Cancelado }

public record PedidoVendaItem(
    string VendedorCodigo, string Numero, DateTime Data, string Cliente,
    double Valor, double Desconto, double Comissao, StatusPedidoVenda Status)
{
    public string StatusLabel => Status switch
    {
        StatusPedidoVenda.Faturado => "Faturado",
        StatusPedidoVenda.Pendente => "Pendente",
        _ => "Cancelado"
    };

    private static string FormatarMoeda(double valor) => $"R$ {valor:N2}".Replace(",", "#").Replace(".", ",").Replace("#", ".");

    public string ValorFormatado => FormatarMoeda(Valor);
    public string DescontoFormatado => Desconto > 0 ? "- " + FormatarMoeda(Desconto) : "—";
    public string ComissaoFormatada => FormatarMoeda(Comissao);
}

public static class PedidoVendaMockData
{
    public static readonly List<PedidoVendaItem> Pedidos = new()
    {
        new("VD0001", "PV-0892", new DateTime(2026, 9, 6), "Construtora Horizonte Ltda",
            18400.00, 920.00, 644.00, StatusPedidoVenda.Faturado),

        new("VD0001", "PV-0871", new DateTime(2026, 9, 2), "Metalúrgica São João SA",
            9750.00, 0, 341.25, StatusPedidoVenda.Pendente),

        new("VD0001", "PV-0860", new DateTime(2026, 8, 30), "Auto Peças Rápida ME",
            3200.00, 160.00, 112.00, StatusPedidoVenda.Faturado),

        new("VD0001", "PV-0847", new DateTime(2026, 8, 27), "Distribuidora Norte Ltda",
            42600.00, 2130.00, 1491.00, StatusPedidoVenda.Faturado),

        new("VD0001", "PV-0831", new DateTime(2026, 8, 22), "Comércio Santos Dumont ME",
            7100.00, 0, 248.50, StatusPedidoVenda.Cancelado),
    };
}