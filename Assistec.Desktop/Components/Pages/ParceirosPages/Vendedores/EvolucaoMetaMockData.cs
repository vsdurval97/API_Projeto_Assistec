using System.Globalization;

namespace Assistec.Desktop.Components.Pages.ParceirosPages.Vendedores;

public record EvolucaoMetaItem(string VendedorCodigo, string Mes, double Realizado, double Meta)
{
    public double Percentual => Meta <= 0 ? 0 : Math.Round(Realizado / Meta * 100, 0);

    public bool AtingiuMeta => Percentual >= 100;

    private static string FormatarCompacto(double valor)
    {
        if (valor >= 1000)
            return (valor / 1000).ToString("0.0", CultureInfo.InvariantCulture) + "k";
        return valor.ToString("0", CultureInfo.InvariantCulture);
    }

    public string RealizadoCompacto => "R$ " + FormatarCompacto(Realizado);
    public string MetaCompacta => "R$ " + FormatarCompacto(Meta);
}

public static class EvolucaoMetaMockData
{
    public static readonly List<EvolucaoMetaItem> Evolucao = new()
    {
        new("VD0001", "Abr/26", 72400, 80000),
        new("VD0001", "Mai/26", 91300, 80000),
        new("VD0001", "Jun/26", 88600, 85000),
        new("VD0001", "Jul/26", 79200, 85000),
        new("VD0001", "Ago/26", 94100, 90000),
        new("VD0001", "Set/26", 61600, 90000),
    };
}