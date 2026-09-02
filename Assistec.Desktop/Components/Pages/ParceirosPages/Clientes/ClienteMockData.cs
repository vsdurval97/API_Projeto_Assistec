namespace Assistec.Desktop.Components.Pages.ParceirosPages.Clientes;

public record ClienteItem(
    string Codigo, string Nome, string Email, string Documento, string TipoLabel,
    string Telefone, string CidadeUf, int Atendimentos, double TotalGasto,
    DateTime UltimaVisita, string CorAvatar, double TotalVendas = 0,
    string? NomeFantasia = null, string InscricaoEstadual = "ISENTO",
    string Logradouro = "", string Numero = "", string Bairro = "", string Cep = "")
{
    public bool Recorrente => Atendimentos >= 5;
    public string Iniciais => string.Concat(Nome.Split(' ').Where(p => p.Length > 0).Take(2).Select(p => p[0])).ToUpperInvariant();
    public string TipoCompleto => TipoLabel == "PF" ? "Pessoa Física" : "Pessoa Jurídica";
    public string EnderecoCompleto => $"{Logradouro}, {Numero} - {Bairro} - {CidadeUf} - CEP {Cep}";

    private static string FormatarMoeda(double valor) => $"R$ {valor:N2}".Replace(",", "#").Replace(".", ",").Replace("#", ".");

    public string TotalGastoFormatado => FormatarMoeda(TotalGasto);
    public string TotalVendasFormatado => FormatarMoeda(TotalVendas);
    public string TotalGeralFormatado => FormatarMoeda(TotalGasto + TotalVendas);
}

public static class ClienteMockData
{
    public static readonly List<ClienteItem> Clientes = new()
{
    new("C0001", "Carlos Silva de Souza", "carlos@email.com", "123.456.789-00", "PF", "(11) 99999-1234", "São Paulo / SP", 7, 2840.50, new DateTime(2026, 8, 28), "#2E7D32", 218.90,
        Logradouro: "Rua das Flores", Numero: "123", Bairro: "Centro", Cep: "01000-000"),
    new("C0002", "Mariana Alencar", "mariana@email.com", "987.654.321-00", "PF", "(19) 98888-5678", "Campinas / SP", 3, 1250.00, new DateTime(2026, 8, 25), "#2E7D32", 0,
        Logradouro: "Av. Brasil", Numero: "456", Bairro: "Jardim Europa", Cep: "13000-000"),
    new("C0003", "Auto Posto Alvorada LTDA", "contato@alvorada.com", "12.345.678/0001-90", "PJ", "(16) 3333-7890", "Ribeirão Preto / SP", 14, 9670.00, new DateTime(2026, 8, 20), "#2E7D32", 540.00,
        NomeFantasia: "Posto Alvorada", InscricaoEstadual: "149.123.456.789", Logradouro: "Av. Presidente Vargas", Numero: "1000", Bairro: "Vila Tibério", Cep: "14020-000"),
    new("C0004", "Clínica Bem Estar S/S", "clinica@bestar.com", "98.765.432/0001-10", "PJ", "(11) 3232-4567", "São Paulo / SP", 9, 4320.00, new DateTime(2026, 8, 18), "#2E7D32", 0,
        NomeFantasia: "Bem Estar", InscricaoEstadual: "ISENTO", Logradouro: "Rua Augusta", Numero: "789", Bairro: "Consolação", Cep: "01305-000"),
    new("C0005", "Fernanda Lima de Oliveira", "fernanda@email.com", "321.654.987-00", "PF", "(13) 97777-3210", "Santos / SP", 2, 580.00, new DateTime(2026, 8, 15), "#C2185B", 0,
        Logradouro: "Av. Ana Costa", Numero: "321", Bairro: "Gonzaga", Cep: "11060-000"),
    new("C0006", "Tech Soluções ME", "tech@solucoes.com", "55.444.333/0001-22", "PJ", "(14) 3456-7890", "Bauru / SP", 6, 3100.00, new DateTime(2026, 8, 10), "#2C6FB0", 320.00,
        NomeFantasia: "Tech Soluções", InscricaoEstadual: "429.987.654.321", Logradouro: "Rua Rio Branco", Numero: "55", Bairro: "Centro", Cep: "17010-000"),
    new("C0007", "Roberto Mendes", "roberto@email.com", "741.258.963-00", "PF", "(15) 96666-4521", "Sorocaba / SP", 4, 760.00, new DateTime(2026, 8, 5), "#C2185B", 0,
        Logradouro: "Rua XV de Novembro", Numero: "741", Bairro: "Centro", Cep: "18010-000"),
    new("C0008", "Mercado Bom Preço LTDA", "mercado@bompreco.com", "77.888.999/0001-55", "PJ", "(11) 4444-1234", "Osasco / SP", 21, 12500.00, new DateTime(2026, 8, 1), "#2E7D32", 0,
        NomeFantasia: "Bom Preço", InscricaoEstadual: "633.111.222.333", Logradouro: "Av. dos Autonomistas", Numero: "77", Bairro: "Vila Yara", Cep: "06020-000"),
};
}