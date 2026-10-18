using System.Globalization;
using Assistec.Desktop.Components.Pages.ParceirosPages.Clientes;

namespace Assistec.Desktop.Components.Pages.OrdemServicoPages.OrdensServico;

public enum StatusOS { Orcamento, AguardandoAprovacao, Aprovada, EmExecucao, AguardandoPeca, Concluida, Cancelada }
public enum PrioridadeOS { Baixa, Normal, Alta, Urgente }
public enum TipoItemOS { Servico, Produto }
public enum TipoEquipamentoOS { Computador, Notebook, Impressora, Outros }

public class ItemOrdemServicoLinha
{
    public TipoItemOS Tipo { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public int Quantidade { get; set; } = 1;
    public double ValorUnitario { get; set; }

    public double Subtotal => Quantidade * ValorUnitario;
    public string SubtotalFormatado => OrdemServicoUi.Moeda(Subtotal);
    public string ValorUnitarioFormatado => OrdemServicoUi.Moeda(ValorUnitario);

    public ItemOrdemServicoLinha Clonar() => (ItemOrdemServicoLinha)MemberwiseClone();
}

public class OrdemServicoItem
{
    public string Numero { get; set; } = string.Empty;
    public DateTime Abertura { get; set; } = DateTime.Today;
    public DateTime? Previsao { get; set; }

    public string ClienteCodigo { get; set; } = string.Empty;
    public string Tecnico { get; set; } = string.Empty;

    public TipoEquipamentoOS TipoEquipamento { get; set; } = TipoEquipamentoOS.Computador;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string NumeroSerie { get; set; } = string.Empty;
    public string DefeitoRelatado { get; set; } = string.Empty;
    public string Diagnostico { get; set; } = string.Empty;

    public string Observacao { get; set; } = string.Empty;
    public string CondicaoPagamento { get; set; } = "À vista";
    public double Desconto { get; set; }

    public StatusOS Status { get; set; } = StatusOS.Orcamento;
    public PrioridadeOS Prioridade { get; set; } = PrioridadeOS.Normal;
    public List<ItemOrdemServicoLinha> Itens { get; set; } = new();

    // O cliente é sempre resolvido pelo código — mesma fonte de Parceiros > Clientes.
    public ClienteItem? Cliente => ClienteMockData.Clientes.FirstOrDefault(c => c.Codigo == ClienteCodigo);
    public string ClienteNome => Cliente?.Nome ?? "Cliente não informado";
    public string ClienteDocumento => Cliente?.Documento ?? string.Empty;
    public string ClienteTelefone => Cliente?.Telefone ?? string.Empty;
    public string ClienteEmail => Cliente?.Email ?? string.Empty;

    public double TotalServicos => Itens.Where(i => i.Tipo == TipoItemOS.Servico).Sum(i => i.Subtotal);
    public double TotalProdutos => Itens.Where(i => i.Tipo == TipoItemOS.Produto).Sum(i => i.Subtotal);
    public double Total => TotalServicos + TotalProdutos - Desconto;

    public string TotalServicosFormatado => OrdemServicoUi.Moeda(TotalServicos);
    public string TotalProdutosFormatado => OrdemServicoUi.Moeda(TotalProdutos);
    public string DescontoFormatado => OrdemServicoUi.Moeda(Desconto);
    public string TotalFormatado => OrdemServicoUi.Moeda(Total);

    public OrdemServicoItem Clonar()
    {
        var copia = (OrdemServicoItem)MemberwiseClone();
        copia.Itens = Itens.Select(i => i.Clonar()).ToList();
        return copia;
    }
}

// Rótulos, cores e formatação compartilhados pelas telas de OS.
public static class OrdemServicoUi
{
    public static string Moeda(double valor) =>
        "R$ " + valor.ToString("N2", CultureInfo.InvariantCulture)
            .Replace(",", "#").Replace(".", ",").Replace("#", ".");

    public static string Rotulo(StatusOS status) => status switch
    {
        StatusOS.Orcamento => "Orçamento",
        StatusOS.AguardandoAprovacao => "Aguardando aprovação",
        StatusOS.Aprovada => "Aprovada",
        StatusOS.EmExecucao => "Em execução",
        StatusOS.AguardandoPeca => "Aguardando peça",
        StatusOS.Concluida => "Concluída",
        _ => "Cancelada"
    };

    public static (string Bg, string Texto) Cor(StatusOS status) => status switch
    {
        StatusOS.Orcamento => ("#F0F2F4", "#5A6B7A"),
        StatusOS.AguardandoAprovacao => ("#FCEFDB", "#D9A441"),
        StatusOS.Aprovada => ("#EAF1FA", "#2C6FB0"),
        StatusOS.EmExecucao => ("#FBE7D8", "#C97A3D"),
        StatusOS.AguardandoPeca => ("#F1E9FA", "#8E5CE0"),
        StatusOS.Concluida => ("#E4F2E5", "#2E7D32"),
        _ => ("#FBE4EC", "#C2185B")
    };

    public static string Rotulo(PrioridadeOS prioridade) => prioridade switch
    {
        PrioridadeOS.Baixa => "Baixa",
        PrioridadeOS.Normal => "Normal",
        PrioridadeOS.Alta => "Alta",
        _ => "Urgente"
    };

    public static string CorPrioridade(PrioridadeOS prioridade) => prioridade switch
    {
        PrioridadeOS.Baixa => "#9AA5B1",
        PrioridadeOS.Normal => "#2C6FB0",
        PrioridadeOS.Alta => "#D9A441",
        _ => "#C0392B"
    };

    public static string Rotulo(TipoEquipamentoOS tipo) => tipo switch
    {
        TipoEquipamentoOS.Computador => "Computador",
        TipoEquipamentoOS.Notebook => "Notebook",
        TipoEquipamentoOS.Impressora => "Impressora",
        _ => "Outros"
    };

    public static string Rotulo(TipoItemOS tipo) => tipo == TipoItemOS.Servico ? "Serviço" : "Produto";

    public static (string Bg, string Texto) Cor(TipoItemOS tipo) =>
        tipo == TipoItemOS.Servico ? ("#EAF1FA", "#2C6FB0") : ("#E4F2E5", "#2E7D32");
}

public static class OrdemServicoMockData
{
    // Hoje é lista fixa; deve virar Usuários (perfil técnico) quando houver backend.
    public static readonly List<string> Tecnicos = new() { "André Martins", "Marcos Paulo", "Juliana Rocha" };

    public static readonly List<string> CondicoesPagamento = new() { "À vista", "21 dias", "28 dias", "30 dias", "30/60 dias" };

    public static readonly List<ItemOrdemServicoLinha> CatalogoItens = new()
    {
        new() { Tipo = TipoItemOS.Servico, Codigo = "SRV-001", Descricao = "Formatação e instalação de sistema operacional", ValorUnitario = 120 },
        new() { Tipo = TipoItemOS.Servico, Codigo = "SRV-002", Descricao = "Diagnóstico de hardware", ValorUnitario = 90 },
        new() { Tipo = TipoItemOS.Servico, Codigo = "SRV-003", Descricao = "Troca de tela de notebook", ValorUnitario = 160 },
        new() { Tipo = TipoItemOS.Servico, Codigo = "SRV-004", Descricao = "Manutenção de impressora laser", ValorUnitario = 180 },
        new() { Tipo = TipoItemOS.Servico, Codigo = "SRV-005", Descricao = "Manutenção de impressora térmica", ValorUnitario = 150 },
        new() { Tipo = TipoItemOS.Servico, Codigo = "SRV-006", Descricao = "Limpeza preventiva e troca de pasta térmica", ValorUnitario = 110 },
        new() { Tipo = TipoItemOS.Servico, Codigo = "SRV-007", Descricao = "Reset e limpeza de almofadas de tinta", ValorUnitario = 140 },
        new() { Tipo = TipoItemOS.Produto, Codigo = "PRD-101", Descricao = "SSD 480GB SATA", ValorUnitario = 229.90 },
        new() { Tipo = TipoItemOS.Produto, Codigo = "PRD-102", Descricao = "Memória DDR4 8GB 3200MHz", ValorUnitario = 189.00 },
        new() { Tipo = TipoItemOS.Produto, Codigo = "PRD-103", Descricao = "Tela LED 15.6\" Slim 40 pinos", ValorUnitario = 389.00 },
        new() { Tipo = TipoItemOS.Produto, Codigo = "PRD-104", Descricao = "Rolo de tração de papel", ValorUnitario = 64.00 },
        new() { Tipo = TipoItemOS.Produto, Codigo = "PRD-105", Descricao = "Unidade de fusão (fusor) 110V", ValorUnitario = 389.90 },
        new() { Tipo = TipoItemOS.Produto, Codigo = "PRD-106", Descricao = "Fonte ATX 500W", ValorUnitario = 219.00 },
        new() { Tipo = TipoItemOS.Produto, Codigo = "PRD-107", Descricao = "Cabeça de impressão térmica", ValorUnitario = 245.00 },
    };

    public static readonly List<OrdemServicoItem> Ordens = new()
    {
        new()
        {
            Numero = "OS-2026-0184", Abertura = new DateTime(2026, 10, 15), Previsao = new DateTime(2026, 10, 20),
            ClienteCodigo = "C0003", Tecnico = "André Martins", Prioridade = PrioridadeOS.Urgente, Status = StatusOS.EmExecucao,
            TipoEquipamento = TipoEquipamentoOS.Impressora, Marca = "HP", Modelo = "LaserJet Pro M404dn", NumeroSerie = "VNB3K12847",
            DefeitoRelatado = "Impressão com manchas pretas e papel enroscando na saída.",
            Diagnostico = "Fusor com desgaste e rolo de tração ressecado.",
            Observacao = "Cliente solicitou prioridade, pois o equipamento é usado na emissão de documentos do posto.",
            CondicaoPagamento = "30 dias", Desconto = 0,
            Itens = new()
            {
                new() { Tipo = TipoItemOS.Servico, Codigo = "SRV-004", Descricao = "Manutenção de impressora laser", Quantidade = 1, ValorUnitario = 180 },
                new() { Tipo = TipoItemOS.Produto, Codigo = "PRD-104", Descricao = "Rolo de tração de papel", Quantidade = 2, ValorUnitario = 64 },
                new() { Tipo = TipoItemOS.Produto, Codigo = "PRD-105", Descricao = "Unidade de fusão (fusor) 110V", Quantidade = 1, ValorUnitario = 389.90 },
            }
        },
        new()
        {
            Numero = "OS-2026-0183", Abertura = new DateTime(2026, 10, 14), Previsao = new DateTime(2026, 10, 19),
            ClienteCodigo = "C0006", Tecnico = "Marcos Paulo", Prioridade = PrioridadeOS.Alta, Status = StatusOS.Aprovada,
            TipoEquipamento = TipoEquipamentoOS.Notebook, Marca = "Dell", Modelo = "Inspiron 15 3520", NumeroSerie = "DL3520-8X41Q",
            DefeitoRelatado = "Notebook não liga e a luz de carga não acende.",
            Diagnostico = "Aguardando diagnóstico técnico.",
            CondicaoPagamento = "28 dias", Desconto = 0,
            Itens = new()
            {
                new() { Tipo = TipoItemOS.Servico, Codigo = "SRV-002", Descricao = "Diagnóstico de hardware", Quantidade = 1, ValorUnitario = 90 },
            }
        },
        new()
        {
            Numero = "OS-2026-0182", Abertura = new DateTime(2026, 10, 13), Previsao = new DateTime(2026, 10, 21),
            ClienteCodigo = "C0002", Tecnico = "André Martins", Prioridade = PrioridadeOS.Normal, Status = StatusOS.AguardandoPeca,
            TipoEquipamento = TipoEquipamentoOS.Computador, Marca = "Positivo", Modelo = "Master C400", NumeroSerie = "PM-C400-55102",
            DefeitoRelatado = "Computador reiniciando sozinho e muito lento.",
            Diagnostico = "SSD com setores defeituosos e excesso de poeira no cooler.",
            CondicaoPagamento = "À vista", Desconto = 20,
            Itens = new()
            {
                new() { Tipo = TipoItemOS.Servico, Codigo = "SRV-001", Descricao = "Formatação e instalação de sistema operacional", Quantidade = 1, ValorUnitario = 120 },
                new() { Tipo = TipoItemOS.Servico, Codigo = "SRV-006", Descricao = "Limpeza preventiva e troca de pasta térmica", Quantidade = 1, ValorUnitario = 110 },
                new() { Tipo = TipoItemOS.Produto, Codigo = "PRD-101", Descricao = "SSD 480GB SATA", Quantidade = 1, ValorUnitario = 229.90 },
            }
        },
        new()
        {
            Numero = "OS-2026-0181", Abertura = new DateTime(2026, 10, 12), Previsao = new DateTime(2026, 10, 22),
            ClienteCodigo = "C0008", Tecnico = "Juliana Rocha", Prioridade = PrioridadeOS.Alta, Status = StatusOS.AguardandoAprovacao,
            TipoEquipamento = TipoEquipamentoOS.Impressora, Marca = "Elgin", Modelo = "L42 Pro", NumeroSerie = "EL42-77104",
            DefeitoRelatado = "Impressora não reconhece a bobina e não imprime o cupom.",
            Diagnostico = string.Empty,
            CondicaoPagamento = "30 dias", Desconto = 0,
            Itens = new()
            {
                new() { Tipo = TipoItemOS.Servico, Codigo = "SRV-005", Descricao = "Manutenção de impressora térmica", Quantidade = 1, ValorUnitario = 150 },
            }
        },
        new()
        {
            Numero = "OS-2026-0180", Abertura = new DateTime(2026, 10, 9), Previsao = new DateTime(2026, 10, 12),
            ClienteCodigo = "C0005", Tecnico = "Juliana Rocha", Prioridade = PrioridadeOS.Normal, Status = StatusOS.Concluida,
            TipoEquipamento = TipoEquipamentoOS.Notebook, Marca = "Lenovo", Modelo = "IdeaPad 3 15IAU7", NumeroSerie = "LN-IP3-90321",
            DefeitoRelatado = "Tela quebrada após queda.",
            Diagnostico = "Painel LCD trincado; placa-mãe e demais componentes funcionando.",
            CondicaoPagamento = "À vista", Desconto = 0,
            Itens = new()
            {
                new() { Tipo = TipoItemOS.Servico, Codigo = "SRV-003", Descricao = "Troca de tela de notebook", Quantidade = 1, ValorUnitario = 160 },
                new() { Tipo = TipoItemOS.Produto, Codigo = "PRD-103", Descricao = "Tela LED 15.6\" Slim 40 pinos", Quantidade = 1, ValorUnitario = 389 },
            }
        },
        new()
        {
            Numero = "OS-2026-0179", Abertura = new DateTime(2026, 10, 8), Previsao = new DateTime(2026, 10, 14),
            ClienteCodigo = "C0001", Tecnico = "Marcos Paulo", Prioridade = PrioridadeOS.Baixa, Status = StatusOS.Orcamento,
            TipoEquipamento = TipoEquipamentoOS.Notebook, Marca = "Acer", Modelo = "Aspire 5 A515", NumeroSerie = "AC-A515-33017",
            DefeitoRelatado = "Lentidão e superaquecimento durante o uso.",
            Diagnostico = string.Empty,
            CondicaoPagamento = "À vista", Desconto = 0,
            Itens = new()
            {
                new() { Tipo = TipoItemOS.Servico, Codigo = "SRV-006", Descricao = "Limpeza preventiva e troca de pasta térmica", Quantidade = 1, ValorUnitario = 110 },
            }
        },
        new()
        {
            Numero = "OS-2026-0178", Abertura = new DateTime(2026, 10, 5), Previsao = new DateTime(2026, 10, 9),
            ClienteCodigo = "C0004", Tecnico = "André Martins", Prioridade = PrioridadeOS.Normal, Status = StatusOS.Cancelada,
            TipoEquipamento = TipoEquipamentoOS.Impressora, Marca = "Epson", Modelo = "L3250", NumeroSerie = "EP-L3250-61884",
            DefeitoRelatado = "Impressão falhando e mensagem de almofadas de tinta cheias.",
            Diagnostico = "Almofadas de tinta saturadas; necessário reset e limpeza.",
            Observacao = "Orçamento recusado: o cliente optou por substituir o equipamento.",
            CondicaoPagamento = "À vista", Desconto = 0,
            Itens = new()
            {
                new() { Tipo = TipoItemOS.Servico, Codigo = "SRV-007", Descricao = "Reset e limpeza de almofadas de tinta", Quantidade = 1, ValorUnitario = 140 },
            }
        },
    };

    public static string ProximoNumero()
    {
        var maior = Ordens
            .Select(o => int.TryParse(o.Numero[^4..], out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max();
        return $"OS-2026-{maior + 1:D4}";
    }

    // Persiste só em memória (mock): substitui se o número já existe, senão insere no topo.
    public static void Salvar(OrdemServicoItem ordem)
    {
        var indice = Ordens.FindIndex(o => o.Numero == ordem.Numero);
        if (indice >= 0) Ordens[indice] = ordem;
        else Ordens.Insert(0, ordem);
    }
}