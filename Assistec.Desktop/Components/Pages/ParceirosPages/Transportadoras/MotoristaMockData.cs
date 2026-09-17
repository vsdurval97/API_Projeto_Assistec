namespace Assistec.Desktop.Components.Pages.ParceirosPages.Transportadoras;

public enum CategoriaCnh { B, C, D, E }
public enum StatusMotorista { Ativo, Inativo }
public enum SituacaoCnh { Valida, AVencer, Vencida }

public record MotoristaItem(
    string Codigo, string Nome, string Cpf, string Cnh, CategoriaCnh Categoria,
    DateTime ValidadeCnh, SituacaoCnh SituacaoCnh, bool HabilitadoMopp, DateTime? ValidadeMopp,
    string? VeiculoPlaca, string TransportadoraCodigo, StatusMotorista Status,
    string Telefone, DateTime? UltimoMdfe, string CorAvatar)
{
    public string Iniciais => string.Concat(Nome.Split(' ').Where(p => p.Length > 0).Take(2).Select(p => p[0])).ToUpperInvariant();

    public string StatusLabel => Status == StatusMotorista.Ativo ? "Ativo" : "Inativo";

    public string CategoriaLabel => Categoria.ToString();
}

public static class MotoristaMockData
{
    public static readonly List<MotoristaItem> Motoristas = new()
    {
        new("MT0001", "Marcos Vinicius Rocha", "123.456.789-00", "00123456789", CategoriaCnh.E,
            new DateTime(2028, 4, 15), SituacaoCnh.Valida, true, new DateTime(2027, 6, 10),
            "BRA2E19", "TR0001", StatusMotorista.Ativo, "(11) 99999-1234",
            new DateTime(2026, 8, 28), "#8B2E2E"),

        new("MT0002", "José Antônio da Silva", "987.654.321-00", "00987654321", CategoriaCnh.D,
            new DateTime(2026, 9, 22), SituacaoCnh.AVencer, false, null,
            "ABC1234", "TR0001", StatusMotorista.Ativo, "(11) 98888-5678",
            new DateTime(2026, 8, 15), "#C97A3D"),

        new("MT0003", "Marcos Vinicius Pereira", "321.654.987-00", "00321654987", CategoriaCnh.C,
            new DateTime(2025, 1, 30), SituacaoCnh.Vencida, false, null,
            "DEF5678", "TR0001", StatusMotorista.Inativo, "(11) 97777-9012",
            new DateTime(2026, 7, 1), "#8B2E2E"),

        new("MT0004", "Patrícia Duarte Lopes", "741.852.963-00", "00741852963", CategoriaCnh.E,
            new DateTime(2027, 11, 8), SituacaoCnh.Valida, true, new DateTime(2026, 3, 15),
            "RJG4H22", "TR0002", StatusMotorista.Ativo, "(21) 96666-3456",
            new DateTime(2026, 8, 25), "#C97A3D"),

        new("MT0005", "Diego Ferreira Santos", "852.963.741-00", "00852963741", CategoriaCnh.B,
            new DateTime(2029, 7, 20), SituacaoCnh.Valida, false, null,
            "RJX1A33", "TR0002", StatusMotorista.Ativo, "(21) 95555-7890",
            new DateTime(2026, 8, 30), "#C97A3D"),

        new("MT0006", "Juliana Prado Mendes", "963.741.852-00", "00963741852", CategoriaCnh.D,
            new DateTime(2027, 5, 12), SituacaoCnh.Valida, false, null,
            "MGK7J55", "TR0004", StatusMotorista.Ativo, "(31) 94444-2345",
            new DateTime(2026, 8, 15), "#C97A3D"),
    };
}