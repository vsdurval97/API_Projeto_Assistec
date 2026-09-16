using System.Globalization;

namespace Assistec.Desktop.Components.Pages.ParceirosPages.Vendedores;

public enum StatusVendedor { Ativo, Inativo, Ferias }

public record VendedorItem(
    string Codigo, string UsuarioCodigo, string Cargo, string Regional,
    string TipoComissao, double PercentualBase, double MetaMensal,
    double TotalVendido, double MetaAtualPercentual, StatusVendedor Status,
    DateTime CadastradoEm, double TotalComissaoHistorico, double TicketMedio)
{
    public string StatusLabel => Status switch
    {
        StatusVendedor.Ativo => "Ativo",
        StatusVendedor.Inativo => "Inativo",
        _ => "Férias"
    };

    public string BaseFormatada => PercentualBase.ToString("0.#", CultureInfo.InvariantCulture) + "%";

    private static string FormatarMoeda(double valor) => $"R$ {valor:N2}".Replace(",", "#").Replace(".", ",").Replace("#", ".");

    public string MetaMensalFormatada => $"R$ {MetaMensal:N0}";
    public string TotalVendidoFormatadoCheio => FormatarMoeda(TotalVendido);
    public string TotalComissaoHistoricoFormatado => FormatarMoeda(TotalComissaoHistorico);
    public string TicketMedioFormatado => FormatarMoeda(TicketMedio);

    private static string FormatarCompacto(double valor)
    {
        if (valor >= 1000)
            return "R$ " + (valor / 1000).ToString("0.#", CultureInfo.InvariantCulture) + "k";
        return $"R$ {valor:N0}";
    }

    public string TotalVendidoFormatado => FormatarCompacto(TotalVendido);
}

public static class VendedorMockData
{
    public static readonly List<VendedorItem> Vendedores = new()
    {
        new("VD0001", "US0003", "Vendedor Sênior", "São Paulo - Capital",
            "Percentual sobre Venda", 3.5, 80000, 487200, 68, StatusVendedor.Ativo,
            new DateTime(2025, 3, 15), 17052.00, 4200.00),

        // Sem mockup de detalhes ainda — TotalComissaoHistorico e TicketMedio são estimativas.
        new("VD0002", "US0008", "Vendedora Pleno", "ABC Paulista",
            "Percentual sobre Lucro", 8, 60000, 264800, 56, StatusVendedor.Ativo,
            new DateTime(2025, 5, 10), 21184.00, 3200.00),
    };
}