using System.Globalization;

namespace Assistec.Desktop.Components.Pages.PedidosVenda;

public enum StatusPedido
{
    Rascunho,
    AguardandoAprovacao,
    Aprovado,
    EmSeparacao,
    Faturado,
    Cancelado
}

public class EnderecoPedido
{
    public string Cep { get; set; } = "";
    public string Logradouro { get; set; } = "";
    public string Numero { get; set; } = "";
    public string Complemento { get; set; } = "";
    public string Bairro { get; set; } = "";
    public string Cidade { get; set; } = "";
    public string Uf { get; set; } = "SP";

    public string Linha =>
        string.IsNullOrWhiteSpace(Logradouro)
            ? ""
            : $"{Logradouro}, {Numero}" + (string.IsNullOrWhiteSpace(Complemento) ? "" : $" — {Complemento}");

    public EnderecoPedido Clonar() => (EnderecoPedido)MemberwiseClone();
}

public class ItemPedidoLinha
{
    public string Codigo { get; set; } = "";
    public string Descricao { get; set; } = "";
    public string Unidade { get; set; } = "UN";
    public int Estoque { get; set; }
    public int Quantidade { get; set; } = 1;
    public decimal ValorUnitario { get; set; }
    public decimal Desconto { get; set; } // percentual (0 a 100)

    public decimal Total => Quantidade * ValorUnitario * (1 - Desconto / 100m);

    public ItemPedidoLinha Clonar() => (ItemPedidoLinha)MemberwiseClone();
}

public class PedidoVendaItem
{
    public string Numero { get; set; } = "";
    public DateTime? Emissao { get; set; }
    public DateTime? Entrega { get; set; }
    public string Cliente { get; set; } = "";
    public string Documento { get; set; } = "";
    public string Telefone { get; set; } = "";
    public string Email { get; set; } = "";
    public EnderecoPedido Endereco { get; set; } = new();
    public string Vendedor { get; set; } = "";
    public string Origem { get; set; } = "Venda direta";
    public StatusPedido Status { get; set; } = StatusPedido.Rascunho;
    public List<ItemPedidoLinha> Itens { get; set; } = new();
    public decimal Desconto { get; set; }
    public decimal Frete { get; set; }
    public string CondicaoPagamento { get; set; } = "À vista";
    public string Transportadora { get; set; } = "";
    public string Observacao { get; set; } = "";

    public decimal Subtotal => Itens.Sum(i => i.Total);
    public decimal Total => Subtotal - Desconto + Frete;

    public PedidoVendaItem Clonar()
    {
        var copia = (PedidoVendaItem)MemberwiseClone();
        copia.Endereco = Endereco.Clonar();
        copia.Itens = Itens.Select(i => i.Clonar()).ToList();
        return copia;
    }
}

public record ClientePvMock(string Nome, string Documento, string Telefone, string Email, EnderecoPedido Endereco);

public static class PedidoVendaUi
{
    public static string Moeda(decimal valor) =>
        "R$ " + valor.ToString("N2", CultureInfo.InvariantCulture)
                     .Replace(",", "X").Replace(".", ",").Replace("X", ".");

    public static string Data(DateTime? data) => data?.ToString("dd/MM/yyyy") ?? "—";

    public static string Rotulo(StatusPedido status) => status switch
    {
        StatusPedido.Rascunho => "Rascunho",
        StatusPedido.AguardandoAprovacao => "Aguardando Aprovação",
        StatusPedido.Aprovado => "Aprovado",
        StatusPedido.EmSeparacao => "Em Separação",
        StatusPedido.Faturado => "Faturado",
        StatusPedido.Cancelado => "Cancelado",
        _ => status.ToString()
    };

    public static (string Fundo, string Texto) Cor(StatusPedido status) => status switch
    {
        StatusPedido.Rascunho => ("#F0F2F4", "#5A6B7A"),
        StatusPedido.AguardandoAprovacao => ("#FCEFDB", "#C97A3D"),
        StatusPedido.Aprovado => ("#EAF1FA", "#2C6FB0"),
        StatusPedido.EmSeparacao => ("#F1E9FA", "#8E5CE0"),
        StatusPedido.Faturado => ("#E4F2E5", "#2E7D32"),
        StatusPedido.Cancelado => ("#FBE4EC", "#C2185B"),
        _ => ("#F0F2F4", "#5A6B7A")
    };
}

public static class PedidoVendaMockData
{
    public static readonly List<string> Vendedores = new() { "Carla Mendes", "Rafael Souza", "Lucas Lima" };

    public static readonly List<string> CondicoesPagamento = new() { "À vista", "21 dias", "28 dias", "30 dias", "30/60 dias" };

    public static readonly List<string> Transportadoras = new()
    {
        "Retirada pelo cliente", "TransVale Logística", "Rápido Sudeste", "Expresso Capital"
    };

    public static readonly List<string> Ufs = new()
    {
        "AC","AL","AP","AM","BA","CE","DF","ES","GO","MA","MT","MS","MG","PA","PB","PR","PE","PI",
        "RJ","RN","RS","RO","RR","SC","SP","SE","TO"
    };

    public static readonly List<ClientePvMock> Clientes = new()
    {
        new("Metalúrgica Horizonte S.A.", "56.789.012/0001-38", "(11) 3555-8420", "compras@horizonte.com.br",
            new EnderecoPedido { Cep = "01310-100", Logradouro = "Avenida Paulista", Numero = "1578", Complemento = "Galpão 3", Bairro = "Bela Vista", Cidade = "São Paulo", Uf = "SP" }),
        new("Transportes Rota Sul Ltda", "42.116.908/0001-55", "(11) 3221-7730", "frota@rotasul.com.br",
            new EnderecoPedido { Cep = "09850-320", Logradouro = "Rua dos Transportadores", Numero = "420", Complemento = "Pátio B", Bairro = "Cooperativa", Cidade = "São Bernardo do Campo", Uf = "SP" }),
        new("Construtora Vale Verde Ltda", "18.445.770/0001-20", "(11) 4128-9060", "obras@valeverde.com.br",
            new EnderecoPedido { Cep = "06454-000", Logradouro = "Alameda Araguaia", Numero = "2044", Complemento = "Conjunto 82", Bairro = "Alphaville", Cidade = "Barueri", Uf = "SP" }),
        new("Padaria São Bento Ltda", "27.109.663/0001-42", "(11) 3384-1190", "compras@saobento.com.br",
            new EnderecoPedido { Cep = "03010-000", Logradouro = "Rua do Gasômetro", Numero = "685", Complemento = "", Bairro = "Brás", Cidade = "São Paulo", Uf = "SP" }),
        new("Oficina Mecânica Dois Irmãos", "33.229.114/0001-09", "(11) 2671-4428", "contato@doisirmaos.com.br",
            new EnderecoPedido { Cep = "03310-000", Logradouro = "Rua Tuiuti", Numero = "1180", Complemento = "Loja 2", Bairro = "Tatuapé", Cidade = "São Paulo", Uf = "SP" }),
    };

    public static readonly List<ItemPedidoLinha> Catalogo = new()
    {
        new() { Codigo = "PRD-006", Descricao = "Rolamento Rígido de Esferas 6205-2RS", Unidade = "UN", Estoque = 84, ValorUnitario = 52 },
        new() { Codigo = "PRD-011", Descricao = "Correia industrial A-42", Unidade = "UN", Estoque = 56, ValorUnitario = 68 },
        new() { Codigo = "PRD-019", Descricao = "Resistência tubular 6000W", Unidade = "UN", Estoque = 13, ValorUnitario = 460 },
        new() { Codigo = "PRD-024", Descricao = "Polia de alumínio 120 mm", Unidade = "UN", Estoque = 17, ValorUnitario = 94 },
        new() { Codigo = "PRD-031", Descricao = "Filtro de ar para compressor industrial", Unidade = "UN", Estoque = 22, ValorUnitario = 78 },
        new() { Codigo = "PRD-044", Descricao = "Bateria automotiva 150Ah", Unidade = "UN", Estoque = 9, ValorUnitario = 1190 },
        new() { Codigo = "PRD-052", Descricao = "Óleo hidráulico ISO VG 68 — 20L", Unidade = "BD", Estoque = 31, ValorUnitario = 398 },
    };

    private static EnderecoPedido End(string cliente) =>
        Clientes.First(c => c.Nome == cliente).Endereco.Clonar();

    private static ClientePvMock Cli(string nome) => Clientes.First(c => c.Nome == nome);

    public static readonly List<PedidoVendaItem> Lista = new()
    {
        new()
        {
            Numero = "PV-2026-0318", Emissao = new(2026, 9, 9), Entrega = new(2026, 9, 12),
            Cliente = Cli("Metalúrgica Horizonte S.A.").Nome, Documento = Cli("Metalúrgica Horizonte S.A.").Documento,
            Telefone = Cli("Metalúrgica Horizonte S.A.").Telefone, Email = Cli("Metalúrgica Horizonte S.A.").Email,
            Endereco = End("Metalúrgica Horizonte S.A."),
            Vendedor = "Carla Mendes", Origem = "ORC-2026-0098", Status = StatusPedido.AguardandoAprovacao,
            Desconto = 120, Frete = 85, CondicaoPagamento = "28 dias", Transportadora = "TransVale Logística",
            Observacao = "Entregar no almoxarifado central, de segunda a sexta até 16h.",
            Itens = new()
            {
                new() { Codigo = "PRD-006", Descricao = "Rolamento Rígido de Esferas 6205-2RS", Unidade = "UN", Estoque = 84, Quantidade = 12, ValorUnitario = 52, Desconto = 0 },
                new() { Codigo = "PRD-031", Descricao = "Filtro de ar para compressor industrial", Unidade = "UN", Estoque = 22, Quantidade = 4, ValorUnitario = 78, Desconto = 5 },
            }
        },
        new()
        {
            Numero = "PV-2026-0317", Emissao = new(2026, 9, 9), Entrega = new(2026, 9, 10),
            Cliente = Cli("Transportes Rota Sul Ltda").Nome, Documento = Cli("Transportes Rota Sul Ltda").Documento,
            Telefone = Cli("Transportes Rota Sul Ltda").Telefone, Email = Cli("Transportes Rota Sul Ltda").Email,
            Endereco = End("Transportes Rota Sul Ltda"),
            Vendedor = "Rafael Souza", Origem = "Venda direta", Status = StatusPedido.Aprovado,
            Desconto = 0, Frete = 0, CondicaoPagamento = "30 dias", Transportadora = "Retirada pelo cliente",
            Observacao = "",
            Itens = new()
            {
                new() { Codigo = "PRD-044", Descricao = "Bateria automotiva 150Ah", Unidade = "UN", Estoque = 9, Quantidade = 2, ValorUnitario = 1190, Desconto = 3 },
            }
        },
        new()
        {
            Numero = "PV-2026-0316", Emissao = new(2026, 9, 8), Entrega = new(2026, 9, 11),
            Cliente = Cli("Construtora Vale Verde Ltda").Nome, Documento = Cli("Construtora Vale Verde Ltda").Documento,
            Telefone = Cli("Construtora Vale Verde Ltda").Telefone, Email = Cli("Construtora Vale Verde Ltda").Email,
            Endereco = End("Construtora Vale Verde Ltda"),
            Vendedor = "Carla Mendes", Origem = "ORC-2026-0096", Status = StatusPedido.EmSeparacao,
            Desconto = 50, Frete = 140, CondicaoPagamento = "30/60 dias", Transportadora = "Rápido Sudeste",
            Observacao = "Separar materiais por obra.",
            Itens = new()
            {
                new() { Codigo = "PRD-011", Descricao = "Correia industrial A-42", Unidade = "UN", Estoque = 56, Quantidade = 20, ValorUnitario = 68, Desconto = 0 },
                new() { Codigo = "PRD-024", Descricao = "Polia de alumínio 120 mm", Unidade = "UN", Estoque = 17, Quantidade = 5, ValorUnitario = 94, Desconto = 0 },
            }
        },
        new()
        {
            Numero = "PV-2026-0315", Emissao = new(2026, 9, 7), Entrega = new(2026, 9, 9),
            Cliente = Cli("Padaria São Bento Ltda").Nome, Documento = Cli("Padaria São Bento Ltda").Documento,
            Telefone = Cli("Padaria São Bento Ltda").Telefone, Email = Cli("Padaria São Bento Ltda").Email,
            Endereco = End("Padaria São Bento Ltda"),
            Vendedor = "Lucas Lima", Origem = "ORC-2026-0095", Status = StatusPedido.Faturado,
            Desconto = 0, Frete = 45, CondicaoPagamento = "À vista", Transportadora = "Expresso Capital",
            Observacao = "NF-e 000.015.892 emitida.",
            Itens = new()
            {
                new() { Codigo = "PRD-019", Descricao = "Resistência tubular 6000W", Unidade = "UN", Estoque = 13, Quantidade = 2, ValorUnitario = 460, Desconto = 0 },
            }
        },
        new()
        {
            Numero = "PV-2026-0314", Emissao = new(2026, 9, 6), Entrega = new(2026, 9, 13),
            Cliente = Cli("Oficina Mecânica Dois Irmãos").Nome, Documento = Cli("Oficina Mecânica Dois Irmãos").Documento,
            Telefone = Cli("Oficina Mecânica Dois Irmãos").Telefone, Email = Cli("Oficina Mecânica Dois Irmãos").Email,
            Endereco = End("Oficina Mecânica Dois Irmãos"),
            Vendedor = "Rafael Souza", Origem = "Venda direta", Status = StatusPedido.Rascunho,
            Desconto = 0, Frete = 0, CondicaoPagamento = "30 dias", Transportadora = "",
            Observacao = "",
            Itens = new()
            {
                new() { Codigo = "PRD-052", Descricao = "Óleo hidráulico ISO VG 68 — 20L", Unidade = "BD", Estoque = 31, Quantidade = 3, ValorUnitario = 398, Desconto = 0 },
            }
        },
    };

    public static PedidoVendaItem? Obter(string numero) =>
        Lista.FirstOrDefault(p => p.Numero == numero);

    public static string ProximoNumero()
    {
        var ultimo = Lista.Max(p => int.Parse(p.Numero[^4..]));
        return $"PV-2026-{ultimo + 1:0000}";
    }

    public static void Salvar(PedidoVendaItem pedido)
    {
        var indice = Lista.FindIndex(p => p.Numero == pedido.Numero);
        if (indice >= 0)
            Lista[indice] = pedido;
        else
            Lista.Insert(0, pedido);
    }
}