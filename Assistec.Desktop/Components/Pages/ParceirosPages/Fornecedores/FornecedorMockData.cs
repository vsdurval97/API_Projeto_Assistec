namespace Assistec.Desktop.Components.Pages.ParceirosPages.Fornecedores;

public record FornecedorItem(
    string Codigo, string NomeFantasia, string RazaoSocial, string Cnpj,
    string NomeContato, string Telefone, string Categoria,
    string PrazoEntrega, int Pedidos, double TotalComprado, DateTime UltimaCompra, string CorAvatar,
    string? InscricaoEstadual = null, string? Email = null,
    string Cep = "", string Logradouro = "", string Numero = "", string Complemento = "",
    string Bairro = "", string Municipio = "", string Uf = "",
    string? CondicaoPagamento = null, string? Site = null, string? Observacoes = null)
{
    public string Iniciais => string.Concat(NomeFantasia.Split(' ').Where(p => p.Length > 0).Take(2).Select(p => p[0])).ToUpperInvariant();

    private static string FormatarMoeda(double valor) => $"R$ {valor:N2}".Replace(",", "#").Replace(".", ",").Replace("#", ".");

    public string TotalCompradoFormatado => FormatarMoeda(TotalComprado);
}

public static class FornecedorMockData
{
    public static readonly List<FornecedorItem> Fornecedores = new()
    {
        new("F0001", "TechParts", "Distribuidora TechParts LTDA", "11.222.333/0001-44",
            "Marcos Ferreira", "(11) 3333-9900", "Peças e Componentes", "2-3 dias úteis",
            32, 48700.00, new DateTime(2026, 8, 27), "#2E7D32"),

        new("F0002", "NacEletro", "Nacional Eletrônicos Comércio ME", "55.666.777/0001-88",
            "Patrícia Ramos", "(21) 98765-4321", "Eletrônicos e Acessórios", "3-5 dias úteis",
            18, 23400.00, new DateTime(2026, 8, 20), "#2C6FB0"),

        new("F0003", "Info Atacado", "Info Atacado S/A", "99.888.777/0001-66",
            "Ricardo Alves", "(41) 3030-5555", "Informática e Periféricos", "1-2 dias úteis",
            45, 61200.00, new DateTime(2026, 8, 15), "#C97A3D"),

        new("F0004", "ConsumBrasil", "Consumíveis Brasil Distribuidora", "22.333.444/0001-55",
            "Juliana Costa", "(31) 4040-2222", "Suprimentos e Consumíveis", "5-7 dias úteis",
            24, 17600.00, new DateTime(2026, 8, 10), "#C2185B"),

        new("F0005", "CE Importados", "Carlos Eduardo Importados ME", "33.444.555/0001-11",
            "Carlos Eduardo", "(48) 99911-8877", "Importados e Especiais", "7-15 dias úteis",
            9, 9800.00, new DateTime(2026, 8, 2), "#8E5CE0"),

        new("F0006", "MegaSupri", "Mega Suprimentos Empresariais LTDA", "77.666.555/0001-33",
            "Fernanda Melo", "(85) 3100-7788", "Suprimentos e Consumíveis", "10-12 dias úteis",
            7, 5300.00, new DateTime(2026, 7, 28), "#2E7D32"),
    };
}