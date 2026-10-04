namespace Assistec.Desktop.Components.Pages.ParceirosPages.Transportadoras;

public enum StatusFrete { Aguardando, EmTransito, Entregue, Cancelado }

public record FreteItem(
    string TransportadoraCodigo, string Numero, DateTime Data, string Origem, string Destino,
    string Placa, string Modalidade, StatusFrete Status, double Valor, string Nfe)
{
    public string StatusLabel => Status switch
    {
        StatusFrete.Aguardando => "Aguardando",
        StatusFrete.EmTransito => "Em trânsito",
        StatusFrete.Entregue => "Entregue",
        _ => "Cancelado"
    };

    private static string FormatarMoeda(double valor) => $"R$ {valor:N2}".Replace(",", "#").Replace(".", ",").Replace("#", ".");

    public string ValorFormatado => FormatarMoeda(Valor);
}

public static class FreteMockData
{
    public static readonly List<FreteItem> Fretes = new()
    {
        new("TR0001", "FRT-0212", new DateTime(2026, 8, 28), "São Paulo / SP", "Curitiba / PR", "BRA2E19", "CIF", StatusFrete.Entregue, 1200.00, "NF-e 000.004.521"),
        new("TR0001", "FRT-0198", new DateTime(2026, 8, 15), "São Paulo / SP", "Porto Alegre / RS", "ABC1234", "FOB", StatusFrete.Entregue, 2100.00, "NF-e 000.004.490"),
        new("TR0001", "FRT-0201", new DateTime(2026, 8, 18), "Campinas / SP", "Rio de Janeiro / RJ", "BRA2E19", "CIF", StatusFrete.EmTransito, 980.00, "NF-e 000.004.498"),
        new("TR0001", "FRT-0215", new DateTime(2026, 9, 1), "São Paulo / SP", "Salvador / BA", "DEF5678", "CIF", StatusFrete.Aguardando, 1850.00, "NF-e 000.004.530"),

        new("TR0002", "FRT-0190", new DateTime(2026, 8, 25), "Rio de Janeiro / RJ", "São Paulo / SP", "RJG4H22", "FOB", StatusFrete.Entregue, 870.00, "NF-e 000.004.480"),
        new("TR0002", "FRT-0175", new DateTime(2026, 8, 10), "Rio de Janeiro / RJ", "Belo Horizonte / MG", "RJG4H22", "CIF", StatusFrete.Entregue, 640.00, "NF-e 000.004.445"),
        new("TR0002", "FRT-0208", new DateTime(2026, 8, 30), "Niterói / RJ", "São Paulo / SP", "RJX1A33", "FOB", StatusFrete.EmTransito, 920.00, "NF-e 000.004.515"),

        new("TR0003", "FRT-0165", new DateTime(2026, 8, 20), "Curitiba / PR", "Manaus / AM", "N/A", "CIF", StatusFrete.Entregue, 3200.00, "NF-e 000.004.430"),
        new("TR0003", "FRT-0172", new DateTime(2026, 8, 5), "Curitiba / PR", "Belém / PA", "N/A", "CIF", StatusFrete.Entregue, 2800.00, "NF-e 000.004.442"),

        new("TR0004", "FRT-0180", new DateTime(2026, 8, 15), "Belo Horizonte / MG", "São Paulo / SP", "MGK7J55", "FOB", StatusFrete.Entregue, 740.00, "NF-e 000.004.455"),
        new("TR0004", "FRT-0200", new DateTime(2026, 8, 25), "Belo Horizonte / MG", "Brasília / DF", "MGP3R11", "CIF", StatusFrete.EmTransito, 890.00, "NF-e 000.004.497"),

        new("TR0005", "FRT-0155", new DateTime(2026, 8, 10), "Campo Grande / MS", "Corumbá / MS", "N/A", "CIF", StatusFrete.Entregue, 1100.00, "NF-e 000.004.400"),
    };
}