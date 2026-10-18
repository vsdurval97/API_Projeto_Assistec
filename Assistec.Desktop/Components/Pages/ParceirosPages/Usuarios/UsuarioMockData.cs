namespace Assistec.Desktop.Components.Pages.ParceirosPages.Usuarios;

public record UsuarioItem(
    string Codigo, string Nome, string Email, string Perfil, bool Ativo, bool Online,
    int TotalAcessos, DateTime UltimoAcesso, DateTime CadastradoEm, string CorAvatar,
    string? Telefone = null)
{
    public string Iniciais => string.Concat(Nome.Split(' ').Where(p => p.Length > 0).Take(2).Select(p => p[0])).ToUpperInvariant();

    public string StatusLabel => Ativo ? "Ativo" : "Inativo";
}

public static class UsuarioMockData
{
    public static readonly List<UsuarioItem> Usuarios = new()
    {
        new("US0001", "Rafael Mendonça", "rafael@assistec.com.br", "Administrador", true, true,
            1248, new DateTime(2026, 9, 7, 14, 32, 0), new DateTime(2025, 1, 1), "#C2185B",
            "(11) 99999-0001"),

        new("US0002", "Camila Ferreira", "camila@assistec.com.br", "Gerente", true, false,
            834, new DateTime(2026, 9, 7, 11, 15, 0), new DateTime(2025, 2, 10), "#8E5CE0",
            "(11) 98888-0002"),

        new("US0003", "Lucas Andrade", "lucas@assistec.com.br", "Vendedor", true, false,
            412, new DateTime(2026, 9, 6, 18, 0, 0), new DateTime(2025, 3, 15), "#2C6FB0",
            "(11) 97777-0003"),

        new("US0004", "Priscila Nunes", "priscila@assistec.com.br", "Financeiro", true, true,
            621, new DateTime(2026, 9, 7, 9, 44, 0), new DateTime(2025, 3, 20), "#2E7D32",
            "(11) 96666-0004"),

        new("US0005", "Henrique Costa", "henrique@assistec.com.br", "Estoque", true, false,
            289, new DateTime(2026, 9, 5, 16, 20, 0), new DateTime(2025, 4, 1), "#C97A3D",
            "(11) 95555-0005"),

        new("US0006", "Tatiane Rocha", "tatiane@assistec.com.br", "Suporte", false, false,
            97, new DateTime(2026, 8, 20, 10, 5, 0), new DateTime(2025, 4, 15), "#C97A3D",
            "(11) 94444-0006"),

        new("US0007", "Fábio Lemos", "fabio@assistec.com.br", "Somente Leitura", false, false,
            43, new DateTime(2026, 7, 10, 8, 30, 0), new DateTime(2025, 5, 1), "#2E7D32",
            "(11) 93333-0007"),

        new("US0008", "Marina Dias", "marina@assistec.com.br", "Vendedor", true, false,
            318, new DateTime(2026, 9, 7, 13, 10, 0), new DateTime(2025, 5, 10), "#8E5CE0",
            "(11) 92222-0008"),
    };
}