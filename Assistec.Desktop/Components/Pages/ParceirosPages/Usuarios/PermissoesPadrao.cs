namespace Assistec.Desktop.Components.Pages.ParceirosPages.Usuarios;

public class PermissaoModuloItem
{
    public string Modulo { get; set; } = string.Empty;
    public bool Visualizar { get; set; }
    public bool Criar { get; set; }
    public bool Editar { get; set; }
    public bool Excluir { get; set; }
}

public static class PermissoesPadrao
{
    private static readonly string[] Modulos =
    {
        "Painel", "Clientes", "Fornecedores", "Contador", "Usuários",
        "Vendedores", "Transportadoras", "Estoque", "Emitente", "Certificado"
    };

    public static List<PermissaoModuloItem> ObterPadrao(string? perfil) => perfil switch
    {
        "Administrador" => Modulos.Select(m => new PermissaoModuloItem
        {
            Modulo = m,
            Visualizar = true,
            Criar = m is "Clientes" or "Fornecedores" or "Vendedores" or "Transportadoras" or "Estoque",
            Editar = m is "Clientes" or "Fornecedores" or "Vendedores" or "Transportadoras" or "Estoque"
        }).ToList(),

        "Gerente" => Modulos.Select(m => new PermissaoModuloItem
        {
            Modulo = m,
            Visualizar = m != "Usuários",
            Criar = m is "Clientes" or "Fornecedores" or "Vendedores" or "Estoque",
            Editar = m is "Clientes" or "Fornecedores" or "Vendedores" or "Estoque"
        }).ToList(),

        "Vendedor" => Modulos.Select(m => new PermissaoModuloItem
        {
            Modulo = m,
            Visualizar = m is "Painel" or "Clientes" or "Vendedores" or "Estoque",
            Criar = m == "Clientes",
            Editar = m == "Clientes"
        }).ToList(),

        "Financeiro" => Modulos.Select(m => new PermissaoModuloItem
        {
            Modulo = m,
            Visualizar = m is "Painel" or "Clientes" or "Fornecedores" or "Contador"
        }).ToList(),

        "Estoque" => Modulos.Select(m => new PermissaoModuloItem
        {
            Modulo = m,
            Visualizar = m is "Painel" or "Fornecedores" or "Estoque",
            Criar = m is "Fornecedores" or "Estoque",
            Editar = m is "Fornecedores" or "Estoque"
        }).ToList(),

        "Suporte" => Modulos.Select(m => new PermissaoModuloItem
        {
            Modulo = m,
            Visualizar = m is "Painel" or "Clientes"
        }).ToList(),

        // "Somente Leitura" e qualquer outro caso: só visualização em tudo.
        _ => Modulos.Select(m => new PermissaoModuloItem
        {
            Modulo = m,
            Visualizar = true
        }).ToList(),
    };
}