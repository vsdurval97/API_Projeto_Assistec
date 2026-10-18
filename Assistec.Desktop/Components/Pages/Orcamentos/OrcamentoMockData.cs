using System.Globalization;

namespace Assistec.Desktop.Components.Pages.Orcamentos;

public enum StatusOrcamento { Rascunho, Enviado, Aprovado, Recusado, Expirado, Convertido }
public enum TipoItemOrcamento { Produto, Servico }

public record ClienteOrcMock(string Codigo, string Nome, string Documento, string Telefone, string Email);

public class ItemOrcamentoLinha
{
    public TipoItemOrcamento Tipo { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public int Quantidade { get; set; } = 1;
    public double ValorUnitario { get; set; }

    public double Subtotal => Quantidade * ValorUnitario;
    public string SubtotalFormatado => OrcamentoUi.Moeda(Subtotal);
    public string ValorUnitarioFormatado => OrcamentoUi.Moeda(ValorUnitario);

    public ItemOrcamentoLinha Clonar() => (ItemOrcamentoLinha)MemberwiseClone();
}

public class OrcamentoItem
{
    public string Numero { get; set; } = string.Empty;
    public DateTime Emissao { get; set; } = DateTime.Today;
    public DateTime? Validade { get; set; }

    public string ClienteCodigo { get; set; } = string.Empty;
    public string Vendedor { get; set; } = string.Empty;
    public StatusOrcamento Status { get; set; } = StatusOrcamento.Rascunho;

    public List<ItemOrcamentoLinha> Itens { get; set; } = new();
    public double Desconto { get; set; }
    public double Frete { get; set; }
    public string CondicaoPagamento { get; set; } = "À vista";
    public string Observacao { get; set; } = string.Empty;

    public ClienteOrcMock? Cliente => OrcamentoMockData.Clientes.FirstOrDefault(c => c.Codigo == ClienteCodigo);
    public string ClienteNome => Cliente?.Nome ?? "Cliente não informado";
    public string ClienteDocumento => Cliente?.Documento ?? string.Empty;
    public string ClienteTelefone => Cliente?.Telefone ?? string.Empty;
    public string ClienteEmail => Cliente?.Email ?? string.Empty;

    public double Subtotal => Itens.Sum(i => i.Subtotal);
    public double Total => Subtotal - Desconto + Frete;

    public string SubtotalFormatado => OrcamentoUi.Moeda(Subtotal);
    public string DescontoFormatado => OrcamentoUi.Moeda(Desconto);
    public string FreteFormatado => OrcamentoUi.Moeda(Frete);
    public string TotalFormatado => OrcamentoUi.Moeda(Total);

    public OrcamentoItem Clonar()
    {
        var copia = (OrcamentoItem)MemberwiseClone();
        copia.Itens = Itens.Select(i => i.Clonar()).ToList();
        return copia;
    }
}

public static class OrcamentoUi
{
    public static string Moeda(double valor) =>
        "R$ " + valor.ToString("N2", CultureInfo.InvariantCulture)
            .Replace(",", "#").Replace(".", ",").Replace("#", ".");

    public static string Rotulo(StatusOrcamento status) => status switch
    {
        StatusOrcamento.Rascunho => "Rascunho",
        StatusOrcamento.Enviado => "Enviado",
        StatusOrcamento.Aprovado => "Aprovado",
        StatusOrcamento.Recusado => "Recusado",
        StatusOrcamento.Expirado => "Expirado",
        _ => "Convertido"
    };

    public static (string Bg, string Texto) Cor(StatusOrcamento status) => status switch
    {
        StatusOrcamento.Rascunho => ("#F0F2F4", "#5A6B7A"),
        StatusOrcamento.Enviado => ("#EAF1FA", "#2C6FB0"),
        StatusOrcamento.Aprovado => ("#E4F2E5", "#2E7D32"),
        StatusOrcamento.Recusado => ("#FBE4EC", "#C2185B"),
        StatusOrcamento.Expirado => ("#FCEFDB", "#D9A441"),
        _ => ("#F1E9FA", "#8E5CE0")
    };

    public static string Rotulo(TipoItemOrcamento tipo) => tipo == TipoItemOrcamento.Servico ? "Serviço" : "Produto";

    public static (string Bg, string Texto) Cor(TipoItemOrcamento tipo) =>
        tipo == TipoItemOrcamento.Servico ? ("#EAF1FA", "#2C6FB0") : ("#E4F2E5", "#2E7D32");
}

public static class OrcamentoMockData
{
    public static readonly List<string> Vendedores = new() { "Carla Mendes", "Rafael Souza", "Lucas Lima" };

    public static readonly List<string> CondicoesPagamento = new() { "À vista", "21 dias", "28 dias", "30 dias", "30/60 dias" };

    public static readonly List<ClienteOrcMock> Clientes = new()
    {
        new("C0001", "Carlos Silva de Souza", "123.456.789-00", "(11) 99999-1234", "carlos@email.com"),
        new("C0002", "Mariana Alencar", "987.654.321-00", "(19) 98888-5678", "mariana@email.com"),
        new("C0003", "Auto Posto Alvorada LTDA", "12.345.678/0001-90", "(16) 3333-7890", "contato@alvorada.com"),
        new("C0004", "Clínica Bem Estar S/S", "98.765.432/0001-10", "(11) 3232-4567", "clinica@bestar.com"),
        new("C0005", "Fernanda Lima de Oliveira", "321.654.987-00", "(13) 97777-3210", "fernanda@email.com"),
        new("C0006", "Tech Soluções ME", "55.444.333/0001-22", "(14) 3456-7890", "tech@solucoes.com"),
        new("C0008", "Mercado Bom Preço LTDA", "77.888.999/0001-55", "(11) 4444-1234", "mercado@bompreco.com"),
    };

    public static readonly List<ItemOrcamentoLinha> CatalogoItens = new()
    {
        new() { Tipo = TipoItemOrcamento.Servico, Codigo = "SRV-001", Descricao = "Formatação e instalação de sistema operacional", ValorUnitario = 120 },
        new() { Tipo = TipoItemOrcamento.Servico, Codigo = "SRV-002", Descricao = "Diagnóstico de hardware", ValorUnitario = 90 },
        new() { Tipo = TipoItemOrcamento.Servico, Codigo = "SRV-003", Descricao = "Troca de tela de notebook", ValorUnitario = 160 },
        new() { Tipo = TipoItemOrcamento.Servico, Codigo = "SRV-004", Descricao = "Manutenção de impressora laser", ValorUnitario = 180 },
        new() { Tipo = TipoItemOrcamento.Servico, Codigo = "SRV-005", Descricao = "Manutenção de impressora térmica", ValorUnitario = 150 },
        new() { Tipo = TipoItemOrcamento.Servico, Codigo = "SRV-006", Descricao = "Limpeza preventiva e troca de pasta térmica", ValorUnitario = 110 },
        new() { Tipo = TipoItemOrcamento.Produto, Codigo = "PRD-101", Descricao = "SSD 480GB SATA", ValorUnitario = 229.90 },
        new() { Tipo = TipoItemOrcamento.Produto, Codigo = "PRD-102", Descricao = "Memória DDR4 8GB 3200MHz", ValorUnitario = 189.00 },
        new() { Tipo = TipoItemOrcamento.Produto, Codigo = "PRD-103", Descricao = "Tela LED 15.6\" Slim 40 pinos", ValorUnitario = 389.00 },
        new() { Tipo = TipoItemOrcamento.Produto, Codigo = "PRD-105", Descricao = "Unidade de fusão (fusor) 110V", ValorUnitario = 389.90 },
        new() { Tipo = TipoItemOrcamento.Produto, Codigo = "PRD-106", Descricao = "Fonte ATX 500W", ValorUnitario = 219.00 },
        new() { Tipo = TipoItemOrcamento.Produto, Codigo = "PRD-107", Descricao = "Cabeça de impressão térmica", ValorUnitario = 245.00 },
        new() { Tipo = TipoItemOrcamento.Produto, Codigo = "PRD-108", Descricao = "Toner compatível para impressora laser", ValorUnitario = 189.00 },
    };

    public static readonly List<OrcamentoItem> Lista = new()
    {
        new()
        {
            Numero = "ORC-2026-0098", Emissao = new DateTime(2026, 10, 15), Validade = new DateTime(2026, 10, 22),
            ClienteCodigo = "C0003", Vendedor = "Carla Mendes", Status = StatusOrcamento.Enviado,
            Desconto = 50, Frete = 30, CondicaoPagamento = "30 dias",
            Observacao = "Prazo de entrega de até 3 dias úteis após a aprovação.",
            Itens = new()
            {
                new() { Tipo = TipoItemOrcamento.Produto, Codigo = "PRD-108", Descricao = "Toner compatível para impressora laser", Quantidade = 4, ValorUnitario = 189 },
                new() { Tipo = TipoItemOrcamento.Servico, Codigo = "SRV-004", Descricao = "Manutenção de impressora laser", Quantidade = 1, ValorUnitario = 180 },
            }
        },
        new()
        {
            Numero = "ORC-2026-0097", Emissao = new DateTime(2026, 10, 14), Validade = new DateTime(2026, 10, 21),
            ClienteCodigo = "C0006", Vendedor = "Rafael Souza", Status = StatusOrcamento.Aprovado,
            Desconto = 0, Frete = 0, CondicaoPagamento = "28 dias",
            Itens = new()
            {
                new() { Tipo = TipoItemOrcamento.Servico, Codigo = "SRV-002", Descricao = "Diagnóstico de hardware", Quantidade = 3, ValorUnitario = 90 },
            }
        },
        new()
        {
            Numero = "ORC-2026-0096", Emissao = new DateTime(2026, 10, 13), Validade = new DateTime(2026, 10, 20),
            ClienteCodigo = "C0002", Vendedor = "Carla Mendes", Status = StatusOrcamento.Rascunho,
            Desconto = 20, Frete = 0, CondicaoPagamento = "À vista",
            Observacao = "Valores sujeitos à disponibilidade de estoque.",
            Itens = new()
            {
                new() { Tipo = TipoItemOrcamento.Produto, Codigo = "PRD-101", Descricao = "SSD 480GB SATA", Quantidade = 1, ValorUnitario = 229.90 },
                new() { Tipo = TipoItemOrcamento.Produto, Codigo = "PRD-102", Descricao = "Memória DDR4 8GB 3200MHz", Quantidade = 2, ValorUnitario = 189 },
                new() { Tipo = TipoItemOrcamento.Servico, Codigo = "SRV-001", Descricao = "Formatação e instalação de sistema operacional", Quantidade = 1, ValorUnitario = 120 },
            }
        },
        new()
        {
            Numero = "ORC-2026-0095", Emissao = new DateTime(2026, 10, 10), Validade = new DateTime(2026, 10, 17),
            ClienteCodigo = "C0008", Vendedor = "Lucas Lima", Status = StatusOrcamento.Convertido,
            Desconto = 0, Frete = 45, CondicaoPagamento = "30 dias",
            Observacao = "Orçamento convertido no pedido PV-2026-0312.",
            Itens = new()
            {
                new() { Tipo = TipoItemOrcamento.Produto, Codigo = "PRD-107", Descricao = "Cabeça de impressão térmica", Quantidade = 2, ValorUnitario = 245 },
                new() { Tipo = TipoItemOrcamento.Servico, Codigo = "SRV-005", Descricao = "Manutenção de impressora térmica", Quantidade = 1, ValorUnitario = 150 },
            }
        },
        new()
        {
            Numero = "ORC-2026-0094", Emissao = new DateTime(2026, 10, 2), Validade = new DateTime(2026, 10, 9),
            ClienteCodigo = "C0001", Vendedor = "Rafael Souza", Status = StatusOrcamento.Expirado,
            Desconto = 0, Frete = 0, CondicaoPagamento = "À vista",
            Itens = new()
            {
                new() { Tipo = TipoItemOrcamento.Servico, Codigo = "SRV-006", Descricao = "Limpeza preventiva e troca de pasta térmica", Quantidade = 1, ValorUnitario = 110 },
                new() { Tipo = TipoItemOrcamento.Produto, Codigo = "PRD-106", Descricao = "Fonte ATX 500W", Quantidade = 1, ValorUnitario = 219 },
            }
        },
        new()
        {
            Numero = "ORC-2026-0093", Emissao = new DateTime(2026, 10, 1), Validade = new DateTime(2026, 10, 8),
            ClienteCodigo = "C0004", Vendedor = "Lucas Lima", Status = StatusOrcamento.Recusado,
            Desconto = 0, Frete = 0, CondicaoPagamento = "30 dias",
            Observacao = "Cliente optou por outro fornecedor.",
            Itens = new()
            {
                new() { Tipo = TipoItemOrcamento.Produto, Codigo = "PRD-108", Descricao = "Toner compatível para impressora laser", Quantidade = 6, ValorUnitario = 189 },
            }
        },
    };

    public static string ProximoNumero()
    {
        var maior = Lista
            .Select(o => int.TryParse(o.Numero[^4..], out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max();
        return $"ORC-2026-{maior + 1:D4}";
    }

    // Persiste só em memória (mock): substitui se o número já existe, senão insere no topo.
    public static void Salvar(OrcamentoItem orcamento)
    {
        var indice = Lista.FindIndex(o => o.Numero == orcamento.Numero);
        if (indice >= 0) Lista[indice] = orcamento;
        else Lista.Insert(0, orcamento);
    }
}