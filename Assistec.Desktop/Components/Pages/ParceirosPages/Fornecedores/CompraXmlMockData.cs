namespace Assistec.Desktop.Components.Pages.ParceirosPages.Fornecedores;

public record CompraXmlItem(
    string FornecedorCodigo, string NumeroNota, DateTime DataEntrada, string Itens, double ValorTotal, string Status)
{
    private static string FormatarMoeda(double valor) => $"R$ {valor:N2}".Replace(",", "#").Replace(".", ",").Replace("#", ".");

    public string ValorTotalFormatado => FormatarMoeda(ValorTotal);
}

public static class CompraXmlMockData
{
    public static readonly List<CompraXmlItem> Compras = new()
    {
        new("F0001", "NF-e 45231", new DateTime(2026, 8, 26),
            "Tela iPhone 13 Pro (5 un.), Bateria Samsung A32 (10 un.)", 4850.00, "Processada"),

        new("F0001", "NF-e 44988", new DateTime(2026, 8, 9),
            "Conector USB-C (10 un.), Câmera traseira iPhone (3 un.)", 2140.00, "Processada"),
    };
}