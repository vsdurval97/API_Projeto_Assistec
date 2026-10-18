namespace Assistec.Desktop.Components.Pages.ParceirosPages.Transportadoras;

public enum CategoriaCnh { A, B, C, D, E, AB, AC, AD, AE }
public enum StatusMotorista { Ativo, Inativo }
public enum SituacaoCnh { Valida, AVencer, Vencida }

public record MotoristaItem(
    string Codigo, string Nome, string Cpf, string Cnh, CategoriaCnh Categoria,
    DateTime ValidadeCnh, bool HabilitadoMopp, DateTime? ValidadeMopp,
    string? VeiculoId, string TransportadoraCodigo, StatusMotorista Status,
    string Telefone, DateTime? UltimoMdfe, string CorAvatar,
    string? DataNascimento = null, string? Email = null, string UfCnh = "",
    string? CertificadoMopp = null, string? NumeroRegistroDetran = null,
    string? DataPrimeiraHabilitacao = null,
    string? Cep = null, string? Logradouro = null, string? Numero = null,
    string? Bairro = null, string? Municipio = null, string? UfResidencia = null)
{
    public string Iniciais => string.Concat(Nome.Split(' ').Where(p => p.Length > 0).Take(2).Select(p => p[0])).ToUpperInvariant();

    public string StatusLabel => Status == StatusMotorista.Ativo ? "Ativo" : "Inativo";

    public string CategoriaLabel => Categoria.ToString();

    public SituacaoCnh SituacaoCnh =>
        ValidadeCnh.Date < DateTime.Today ? SituacaoCnh.Vencida
        : ValidadeCnh.Date <= DateTime.Today.AddDays(90) ? SituacaoCnh.AVencer
        : SituacaoCnh.Valida;
}

public static class MotoristaMockData
{
    public static readonly List<MotoristaItem> Motoristas = new()
    {
        new("MT0001", "Marcos Vinicius Rocha", "123.456.789-00", "00123456789", CategoriaCnh.E,
            new DateTime(2028, 4, 15), true, new DateTime(2027, 6, 10),
            "VE0001", "TR0001", StatusMotorista.Ativo, "(11) 99999-1234",
            new DateTime(2026, 8, 28), "#8B2E2E",
            "12/03/1985", "marcos.rocha@email.com", "SP", "MOPP-2024-SP-00234"),

        new("MT0002", "José Antônio da Silva", "987.654.321-00", "00987654321", CategoriaCnh.D,
            new DateTime(2026, 9, 22), false, null,
            "VE0002", "TR0001", StatusMotorista.Ativo, "(11) 98888-5678",
            new DateTime(2026, 8, 15), "#C97A3D",
            "28/07/1978", "jose.silva@email.com", "SP"),

        new("MT0003", "Marcos Vinicius Pereira", "321.654.987-00", "00321654987", CategoriaCnh.C,
            new DateTime(2025, 1, 30), false, null,
            "VE0003", "TR0001", StatusMotorista.Inativo, "(11) 97777-9012",
            new DateTime(2026, 7, 1), "#8B2E2E",
            "05/11/1990", "mvpereira@email.com", "SP"),

        new("MT0004", "Patrícia Duarte Lopes", "741.852.963-00", "00741852963", CategoriaCnh.E,
            new DateTime(2027, 11, 8), true, new DateTime(2026, 3, 15),
            "VE0004", "TR0002", StatusMotorista.Ativo, "(21) 96666-3456",
            new DateTime(2026, 8, 25), "#C97A3D",
            "19/02/1983", "patricia.lopes@email.com", "RJ", "MOPP-2023-RJ-00512"),

        new("MT0005", "Diego Ferreira Santos", "852.963.741-00", "00852963741", CategoriaCnh.B,
            new DateTime(2029, 7, 20), false, null,
            "VE0005", "TR0002", StatusMotorista.Ativo, "(21) 95555-7890",
            new DateTime(2026, 8, 30), "#C97A3D",
            "14/06/1995", "diego.fs@email.com", "RJ"),

        new("MT0006", "Juliana Prado Mendes", "963.741.852-00", "00963741852", CategoriaCnh.D,
            new DateTime(2027, 5, 12), false, null,
            "VE0006", "TR0004", StatusMotorista.Ativo, "(31) 94444-2345",
            new DateTime(2026, 8, 15), "#C97A3D",
            "30/09/1987", "juliana.prado@email.com", "MG"),
    };
}