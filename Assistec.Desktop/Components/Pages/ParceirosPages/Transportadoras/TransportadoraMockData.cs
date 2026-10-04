namespace Assistec.Desktop.Components.Pages.ParceirosPages.Transportadoras;

public enum ModalTransporte { Rodoviario, Aereo, Aquaviario, Ferroviario }

public record TransportadoraItem(
    string Codigo, string NomeFantasia, string RazaoSocial, string Cnpj, string Rntrc,
    ModalTransporte Modal, string NomeContato, string Telefone,
    int Fretes, double TotalFaturado, DateTime UltimoFrete, string CorAvatar,
    string Ie = "", string? Email = null,
    string Cep = "", string Logradouro = "", string Numero = "", string Complemento = "",
    string Bairro = "", string Municipio = "", string Uf = "")
{
    public string Iniciais => string.Concat(NomeFantasia.Split(' ').Where(p => p.Length > 0).Take(2).Select(p => p[0])).ToUpperInvariant();

    public string ModalLabel => Modal switch
    {
        ModalTransporte.Rodoviario => "Rodoviário",
        ModalTransporte.Aereo => "Aéreo",
        ModalTransporte.Aquaviario => "Aquaviário",
        _ => "Ferroviário"
    };

    private static string FormatarMoeda(double valor) => $"R$ {valor:N2}".Replace(",", "#").Replace(".", ",").Replace("#", ".");

    public string TotalFaturadoFormatado => FormatarMoeda(TotalFaturado);
}

public static class TransportadoraMockData
{
    public static readonly List<TransportadoraItem> Transportadoras = new()
    {
        new("TR0001", "RápidoPaul", "Rápido Paulista Transportes LTDA", "11.222.333/0001-44", "01234567",
            ModalTransporte.Rodoviario, "Marcos Vinicius", "(11) 3333-9900",
            48, 32400.00, new DateTime(2026, 8, 28), "#C97A3D",
            "111.222.333.444", "operacoes@rapidopaul.com.br",
            "04794-000", "Av. das Nações Unidas", "14401", "", "Vila Gertrudes", "São Paulo", "SP"),

        new("TR0002", "LogNac Express", "Logística Nacional Express S/A", "55.666.777/0001-88", "09876543",
            ModalTransporte.Rodoviario, "Patrícia Duarte", "(21) 98765-4321",
            31, 18700.00, new DateTime(2026, 8, 25), "#C97A3D",
            "ISENTO", "comercial@lognac.com.br",
            "20040-080", "Rua Presidente Vargas", "200", "", "Centro", "Rio de Janeiro", "RJ"),

        new("TR0003", "AeroCargo Sul", "Aero Cargo Sul Ltda", "99.888.777/0001-66", "11223344",
            ModalTransporte.Aereo, "Ricardo Holanda", "(41) 3030-5555",
            12, 9200.00, new DateTime(2026, 8, 20), "#C97A3D",
            "999.888.777.666", "frete@aerocargosul.com.br",
            "80530-230", "Rua Amintas de Barros", "55", "", "Centro Cívico", "Curitiba", "PR"),

        new("TR0004", "TransMinas", "TransMinas Cargas ME", "22.333.444/0001-55", "55443322",
            ModalTransporte.Rodoviario, "Juliana Prado", "(31) 4040-2222",
            19, 11300.00, new DateTime(2026, 8, 15), "#C97A3D",
            "222.333.444.555", "fretes@transminas.com",
            "30130-005", "Av. Afonso Pena", "1500", "", "Centro", "Belo Horizonte", "MG"),

        new("TR0005", "Fluvial CO", "Fluvial Centro-Oeste Navegação ME", "33.444.555/0001-11", "77665544",
            ModalTransporte.Aquaviario, "Diego Matos", "(67) 99911-8877",
            7, 5600.00, new DateTime(2026, 8, 10), "#C97A3D",
            "333.444.555.111", "fluvialco@gmail.com",
            "79031-902", "Av. Afonso Pena", "7000", "", "Jardim dos Estados", "Campo Grande", "MS"),
    };
}