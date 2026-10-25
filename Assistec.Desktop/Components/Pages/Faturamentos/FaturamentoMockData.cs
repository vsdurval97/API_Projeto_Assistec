using System.Globalization;

namespace Assistec.Desktop.Components.Pages.Faturamentos;

public enum StatusNFe { Rascunho, Validada, Assinada, Autorizada, Rejeitada, Cancelada, Inutilizada }
public enum AcaoFiscal { Espelho, Emitir }
public enum TipoDocumentoFiscal { Espelho, Danfe }
public enum TipoOrigemNFe { PedidoVenda, Orcamento, OrdemServico }
public enum AcaoConfirmacaoNFe { Excluir, Inutilizar }

public record ResultadoNFe(NFeItem Nota, AcaoFiscal? Acao);

public record SituacaoTributaria(string Codigo, string Descricao)
{
    public bool TemSt => Codigo is "10" or "30" or "60" or "500" or "900";
    public string Regime => Codigo.Length == 3 ? "CSOSN — Simples Nacional" : "CST — Regime normal";
}

public class DestinatarioNFe
{
    public string Nome { get; set; } = "";
    public string Documento { get; set; } = "";
    public string Ie { get; set; } = "";
    public string Email { get; set; } = "";
    public string Telefone { get; set; } = "";
    public string Cep { get; set; } = "";
    public string Logradouro { get; set; } = "";
    public string Numero { get; set; } = "";
    public string Complemento { get; set; } = "";
    public string Bairro { get; set; } = "";
    public string Cidade { get; set; } = "";
    public string Uf { get; set; } = "SP";

    public string EnderecoCompleto =>
        string.IsNullOrWhiteSpace(Logradouro)
            ? ""
            : $"{Logradouro}, {Numero}{(string.IsNullOrWhiteSpace(Complemento) ? "" : $" — {Complemento}")} · {Bairro} · {Cidade}/{Uf} · CEP {Cep}";

    public DestinatarioNFe Clonar() => (DestinatarioNFe)MemberwiseClone();
}

public class ItemNFeLinha
{
    public string Codigo { get; set; } = "";
    public string Descricao { get; set; } = "";
    public string Ncm { get; set; } = "";
    public string Cest { get; set; } = "";
    public string Cfop { get; set; } = "5102";
    public string Unidade { get; set; } = "UN";
    public decimal Quantidade { get; set; } = 1;
    public decimal ValorUnitario { get; set; }
    public string Cst { get; set; } = "00";
    public decimal Icms { get; set; }
    public decimal BaseIcmsSt { get; set; }
    public decimal IcmsSt { get; set; }
    public decimal BaseFcp { get; set; }
    public decimal AliquotaFcp { get; set; }
    public decimal ValorFcp { get; set; }
    public decimal BaseFcpSt { get; set; }
    public decimal AliquotaFcpSt { get; set; }
    public decimal ValorFcpSt { get; set; }
    public decimal Ipi { get; set; }
    public decimal Pis { get; set; }
    public decimal Cofins { get; set; }
    public decimal Ibs { get; set; }
    public decimal Cbs { get; set; }
    public decimal Is { get; set; }
    public decimal FreteItem { get; set; }
    public decimal SeguroItem { get; set; }
    public decimal OutrasDespesasItem { get; set; }
    public decimal DescontoItem { get; set; }

    public decimal Total => Math.Round(Quantidade * ValorUnitario, 2);
    public decimal IcmsStTotal => IcmsSt * Quantidade;
    public decimal FcpTotal => ValorFcp + ValorFcpSt;
    public bool TemRateio => FreteItem > 0 || SeguroItem > 0 || OutrasDespesasItem > 0 || DescontoItem > 0;

    public void DefinirTotal(decimal total) => ValorUnitario = Quantidade > 0 ? total / Quantidade : 0;

    public void AplicarCst(string codigo)
    {
        Cst = codigo;
        if (codigo == "900")
        {
            if (BaseIcmsSt == 0) BaseIcmsSt = Total * 1.3m;
            if (IcmsSt == 0) IcmsSt = Total * 0.06m;
        }
    }

    public ItemNFeLinha Clonar() => (ItemNFeLinha)MemberwiseClone();
}

public class NFeItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Numero { get; set; } = "";
    public string Serie { get; set; } = "1";
    public DateTime? Emissao { get; set; }
    public string Chave { get; set; } = "";
    public string Protocolo { get; set; } = "";
    public string Natureza { get; set; } = "Venda de mercadoria adquirida de terceiros";
    public string TipoOperacao { get; set; } = "Saída";
    public string Finalidade { get; set; } = "NF-e normal";
    public string Presenca { get; set; } = "Operação presencial";
    public bool ConsumidorFinal { get; set; }
    public StatusNFe Status { get; set; } = StatusNFe.Rascunho;
    public DestinatarioNFe Destinatario { get; set; } = new();
    public List<ItemNFeLinha> Itens { get; set; } = new();
    public decimal Desconto { get; set; }
    public decimal Frete { get; set; }
    public decimal Seguro { get; set; }
    public decimal OutrasDespesas { get; set; }
    public string ModalidadeFrete { get; set; } = "Sem ocorrência de transporte";
    public string Transportadora { get; set; } = "";
    public int Volumes { get; set; }
    public string Especie { get; set; } = "";
    public decimal PesoLiquido { get; set; }
    public decimal PesoBruto { get; set; }
    public string FormaPagamento { get; set; } = "PIX";
    public int Parcelas { get; set; } = 1;
    public string InformacoesComplementares { get; set; } = "";
    public string? TipoDevolucao { get; set; }
    public string? DocumentoReferenciado { get; set; }
    public string? MotivoDevolucao { get; set; }

    public decimal Subtotal => Itens.Sum(i => i.Total);
    public decimal TotalIcms => Itens.Sum(i => i.Total * i.Icms / 100m);
    public decimal TotalIpi => Itens.Sum(i => i.Total * i.Ipi / 100m);
    public decimal TotalPis => Itens.Sum(i => i.Total * i.Pis / 100m);
    public decimal TotalCofins => Itens.Sum(i => i.Total * i.Cofins / 100m);
    public decimal TotalIbs => Itens.Sum(i => i.Total * i.Ibs / 100m);
    public decimal TotalCbs => Itens.Sum(i => i.Total * i.Cbs / 100m);
    public decimal TotalIs => Itens.Sum(i => i.Total * i.Is / 100m);
    public decimal TotalIcmsSt => Itens.Sum(i => i.IcmsStTotal);
    public decimal TotalFcp => Itens.Sum(i => i.FcpTotal);
    public decimal Total => Subtotal - Desconto + Frete + Seguro + OutrasDespesas;

    public NFeItem Clonar()
    {
        var copia = (NFeItem)MemberwiseClone();
        copia.Destinatario = Destinatario.Clonar();
        copia.Itens = Itens.Select(i => i.Clonar()).ToList();
        return copia;
    }
}

public class DocumentoOrigemNFe
{
    public TipoOrigemNFe Tipo { get; set; }
    public string Numero { get; set; } = "";
    public string Data { get; set; } = "";
    public string Cliente { get; set; } = "";
    public decimal Total { get; set; }
    public DestinatarioNFe Destinatario { get; set; } = new();
    public List<ItemNFeLinha> Itens { get; set; } = new();
    public decimal Frete { get; set; }
    public string FormaPagamento { get; set; } = "";
    public int Parcelas { get; set; } = 1;
}

public class DocumentoDevolucaoNFe
{
    public string Tipo { get; set; } = "Saída"; // "Saída" ou "Entrada"
    public string Numero { get; set; } = "";
    public string Serie { get; set; } = "1";
    public string Emissao { get; set; } = "";
    public string Chave { get; set; } = "";
    public DestinatarioNFe Contraparte { get; set; } = new();
    public List<ItemNFeLinha> Itens { get; set; } = new();
}

public static class FaturamentoUi
{
    public static string Moeda(decimal valor)
    {
        var texto = "R$ " + Math.Abs(valor).ToString("N2", CultureInfo.InvariantCulture)
                                .Replace(",", "X").Replace(".", ",").Replace("X", ".");
        return valor < 0 ? "− " + texto : texto;
    }

    public static string Num(decimal valor) =>
        valor.ToString("0.##", CultureInfo.InvariantCulture).Replace('.', ',');

    public static string DataHora(DateTime? data) => data?.ToString("dd/MM/yyyy HH:mm") ?? "—";

    public static string Data(DateTime? data) => data?.ToString("dd/MM/yyyy") ?? "—";

    public static string Rotulo(StatusNFe status) => status.ToString();

    public static string Rotulo(TipoOrigemNFe tipo) => tipo switch
    {
        TipoOrigemNFe.PedidoVenda => "Pedido de Venda",
        TipoOrigemNFe.Orcamento => "Orçamento",
        _ => "Ordem de Serviço"
    };

    public static (string Fundo, string Texto) Cor(StatusNFe status) => status switch
    {
        StatusNFe.Rascunho => ("#F0F2F4", "#5A6B7A"),
        StatusNFe.Validada => ("#EAF1FA", "#2C6FB0"),
        StatusNFe.Assinada => ("#F1E9FA", "#8E5CE0"),
        StatusNFe.Autorizada => ("#E4F2E5", "#2E7D32"),
        StatusNFe.Rejeitada => ("#FBE4EC", "#C2185B"),
        StatusNFe.Cancelada => ("#FBE7D8", "#C97A3D"),
        StatusNFe.Inutilizada => ("#E3E6EA", "#5A6B7A"),
        _ => ("#F0F2F4", "#5A6B7A")
    };
}

public static class FaturamentoMockData
{
    // ---------- listas de apoio ----------
    public static readonly List<string> Finalidades = new()
    {
        "NF-e normal", "NF-e complementar", "NF-e de ajuste", "Devolução de mercadoria"
    };

    public static readonly List<string> Presencas = new()
    {
        "Operação presencial",
        "Operação não presencial — Internet",
        "Operação não presencial — Teleatendimento",
        "Operação não presencial — Outros"
    };

    public static readonly List<string> Modalidades = new()
    {
        "Sem ocorrência de transporte",
        "Contratação do frete por conta do remetente",
        "Contratação do frete por conta do destinatário",
        "Transporte próprio por conta do remetente",
        "Transporte próprio por conta do destinatário"
    };

    public static readonly List<string> FormasPagamento = new()
    {
        "PIX", "Dinheiro", "Cartão de crédito", "Boleto bancário", "Duplicata mercantil", "Sem pagamento"
    };

    public static readonly List<string> Ufs = new()
    {
        "AC","AL","AP","AM","BA","CE","DF","ES","GO","MA","MT","MS","MG","PA","PB","PR","PE","PI",
        "RJ","RN","RS","RO","RR","SC","SP","SE","TO"
    };

    public static readonly List<SituacaoTributaria> Situacoes = new()
    {
        new("00", "Tributada integralmente"),
        new("10", "Tributada com cobrança de ICMS-ST"),
        new("30", "Isenta/não tributada com ICMS-ST"),
        new("60", "ICMS cobrado anteriormente por ST"),
        new("90", "Outras"),
        new("102", "Simples Nacional sem crédito"),
        new("500", "ICMS cobrado anteriormente por ST"),
        new("900", "Outros (Simples Nacional)"),
    };

    // ---------- catálogo fiscal ----------
    public static readonly List<ItemNFeLinha> Catalogo = new()
    {
        new() { Codigo = "PRD-006", Descricao = "Rolamento Rígido de Esferas 6205-2RS", Ncm = "8482.10.10", Cest = "01.050.00", Cfop = "5102", Unidade = "UN", ValorUnitario = 52, Cst = "00", Icms = 18, Pis = 1.65m, Cofins = 7.6m, Ibs = 0.1m, Cbs = 0.9m },
        new() { Codigo = "PRD-011", Descricao = "Correia industrial A-42", Ncm = "4010.32.00", Cest = "01.020.00", Cfop = "5102", Unidade = "UN", ValorUnitario = 68, Cst = "00", Icms = 18, Pis = 1.65m, Cofins = 7.6m, Ibs = 0.1m, Cbs = 0.9m },
        new() { Codigo = "PRD-019", Descricao = "Resistência tubular 6000W", Ncm = "8516.80.90", Cest = "", Cfop = "5102", Unidade = "UN", ValorUnitario = 460, Cst = "00", Icms = 18, Ipi = 5, Pis = 1.65m, Cofins = 7.6m, Ibs = 0.1m, Cbs = 0.9m },
        new() { Codigo = "PRD-024", Descricao = "Polia de alumínio 120 mm", Ncm = "8483.50.10", Cest = "", Cfop = "5102", Unidade = "UN", ValorUnitario = 94, Cst = "00", Icms = 18, Pis = 1.65m, Cofins = 7.6m, Ibs = 0.1m, Cbs = 0.9m },
        new() { Codigo = "PRD-044", Descricao = "Bateria automotiva 150Ah", Ncm = "8507.10.90", Cest = "01.053.00", Cfop = "5405", Unidade = "UN", ValorUnitario = 1190, Cst = "900", Icms = 12, BaseIcmsSt = 1547, IcmsSt = 71.4m, BaseFcpSt = 1547, AliquotaFcpSt = 2, ValorFcpSt = 30.94m, Pis = 1.65m, Cofins = 7.6m, Ibs = 0.1m, Cbs = 0.9m },
        new() { Codigo = "PRD-052", Descricao = "Óleo hidráulico ISO VG 68 — 20L", Ncm = "2710.19.32", Cest = "06.008.00", Cfop = "5405", Unidade = "BD", ValorUnitario = 398, Cst = "60", Icms = 0, Pis = 1.65m, Cofins = 7.6m, Ibs = 0.1m, Cbs = 0.9m },
    };

    private static ItemNFeLinha Cat(int indice, decimal quantidade, Action<ItemNFeLinha>? ajuste = null)
    {
        var item = Catalogo[indice].Clonar();
        item.Quantidade = quantidade;
        ajuste?.Invoke(item);
        return item;
    }

    private static DestinatarioNFe D(string nome, string doc, string ie, string email, string tel, string cep,
        string log, string num, string comp, string bairro, string cidade, string uf = "SP") => new()
    {
        Nome = nome, Documento = doc, Ie = ie, Email = email, Telefone = tel, Cep = cep,
        Logradouro = log, Numero = num, Complemento = comp, Bairro = bairro, Cidade = cidade, Uf = uf
    };

    private static DestinatarioNFe DSaoBento() => D("Padaria São Bento Ltda", "27.109.663/0001-42", "114.228.771.110", "compras@saobento.com.br", "(11) 3384-1190", "03010-000", "Rua do Gasômetro", "685", "", "Brás", "São Paulo");
    private static DestinatarioNFe DRotaSul() => D("Transportes Rota Sul Ltda", "42.116.908/0001-55", "635.772.910.118", "frota@rotasul.com.br", "(11) 3221-7730", "09850-320", "Rua dos Transportadores", "420", "Pátio B", "Cooperativa", "São Bernardo do Campo");
    private static DestinatarioNFe DValeVerde() => D("Construtora Vale Verde Ltda", "18.445.770/0001-20", "206.441.908.114", "obras@valeverde.com.br", "(11) 4128-9060", "06454-000", "Alameda Araguaia", "2044", "Conjunto 82", "Alphaville", "Barueri");
    private static DestinatarioNFe DNovaEra() => D("Comercial Nova Era Ltda", "09.881.442/0001-70", "ISENTO", "fiscal@novaera.com.br", "(11) 3011-4490", "04101-300", "Rua Vergueiro", "890", "", "Liberdade", "São Paulo");
    private static DestinatarioNFe DHorizonte(string email = "compras@horizonte.com.br") => D("Metalúrgica Horizonte S.A.", "56.789.012/0001-38", "149.882.771.110", email, "(11) 3555-8420", "01310-100", "Avenida Paulista", "1578", "Galpão 3", "Bela Vista", "São Paulo");

    // ---------- notas fiscais ----------
    public static readonly List<NFeItem> Notas = new()
    {
        new()
        {
            Id = "nfe-15892", Numero = "000.015.892", Emissao = new(2026, 9, 9, 10, 42, 0),
            Chave = "35260912345678000190550010000158921000158925", Protocolo = "135260001987654",
            Status = StatusNFe.Autorizada, Destinatario = DSaoBento(), Itens = new() { Cat(2, 2) },
            Frete = 45, ModalidadeFrete = "Contratação do frete por conta do remetente", Transportadora = "Expresso Capital",
            Volumes = 1, Especie = "Caixa", PesoLiquido = 4.8m, PesoBruto = 5.2m, FormaPagamento = "Dinheiro",
            InformacoesComplementares = "Pedido PV-2026-0315. Tributos aproximados conforme Lei 12.741/2012."
        },
        new()
        {
            Id = "nfe-15891", Numero = "000.015.891", Emissao = new(2026, 9, 9, 9, 18, 0),
            Presenca = "Operação não presencial — Internet", Status = StatusNFe.Assinada,
            Destinatario = DRotaSul(), Itens = new() { Cat(4, 2) },
            FormaPagamento = "Duplicata mercantil", InformacoesComplementares = "Pedido PV-2026-0317."
        },
        new()
        {
            Id = "nfe-15890", Numero = "000.015.890", Emissao = new(2026, 9, 8, 16, 5, 0),
            Presenca = "Operação não presencial — Outros", Status = StatusNFe.Rascunho,
            Destinatario = DValeVerde(), Itens = new() { Cat(1, 20), Cat(3, 5) },
            Desconto = 50, Frete = 140, ModalidadeFrete = "Contratação do frete por conta do remetente", Transportadora = "Rápido Sudeste",
            Volumes = 3, Especie = "Caixa", PesoLiquido = 18.5m, PesoBruto = 20.1m,
            FormaPagamento = "Duplicata mercantil", Parcelas = 2, InformacoesComplementares = "Pedido PV-2026-0316."
        },
        new()
        {
            Id = "nfe-15889", Numero = "000.015.889", Emissao = new(2026, 9, 8, 13, 22, 0),
            ConsumidorFinal = true, Status = StatusNFe.Rejeitada,
            Destinatario = DNovaEra(), Itens = new() { Cat(0, 6) },
            Frete = 30, ModalidadeFrete = "Contratação do frete por conta do remetente", Transportadora = "Entrega Própria",
            Volumes = 1, Especie = "Caixa", PesoLiquido = 2.4m, PesoBruto = 2.7m, FormaPagamento = "PIX",
            InformacoesComplementares = "Rejeição 234: IE do destinatário não vinculada ao CNPJ."
        },
    };

    // ---------- documentos de origem (importação) ----------
    public static readonly List<DocumentoOrigemNFe> DocumentosOrigem = new()
    {
        new() { Tipo = TipoOrigemNFe.PedidoVenda, Numero = "PV-2026-0318", Data = "09/09/2026", Cliente = "Metalúrgica Horizonte S.A.", Total = 901.4m, Destinatario = DHorizonte(), Itens = new() { Cat(0, 12), Cat(4, 4) }, Frete = 85, FormaPagamento = "Duplicata mercantil", Parcelas = 1 },
        new() { Tipo = TipoOrigemNFe.PedidoVenda, Numero = "PV-2026-0317", Data = "09/09/2026", Cliente = "Transportes Rota Sul Ltda", Total = 2380, Destinatario = DRotaSul(), Itens = new() { Cat(4, 2) }, Frete = 0, FormaPagamento = "Duplicata mercantil", Parcelas = 1 },
        new() { Tipo = TipoOrigemNFe.Orcamento, Numero = "ORC-2026-0098", Data = "09/09/2026", Cliente = "Metalúrgica Horizonte S.A.", Total = 1074, Destinatario = DHorizonte(), Itens = new() { Cat(0, 12) }, Frete = 120, FormaPagamento = "Duplicata mercantil", Parcelas = 1 },
        new() { Tipo = TipoOrigemNFe.Orcamento, Numero = "ORC-2026-0096", Data = "08/09/2026", Cliente = "Construtora Vale Verde Ltda", Total = 1840, Destinatario = DValeVerde(), Itens = new() { Cat(1, 20), Cat(3, 5) }, Frete = 90, FormaPagamento = "Duplicata mercantil", Parcelas = 2 },
        new() { Tipo = TipoOrigemNFe.OrdemServico, Numero = "OS-2026-0184", Data = "09/09/2026", Cliente = "Metalúrgica Horizonte S.A.", Total = 584, Destinatario = DHorizonte("manutencao@horizonte.com.br"), Itens = new() { Cat(0, 2) }, Frete = 0, FormaPagamento = "Duplicata mercantil", Parcelas = 1 },
        new() { Tipo = TipoOrigemNFe.OrdemServico, Numero = "OS-2026-0182", Data = "08/09/2026", Cliente = "Construtora Vale Verde Ltda", Total = 436, Destinatario = DValeVerde(), Itens = new() { Cat(1, 2) }, Frete = 0, FormaPagamento = "Dinheiro", Parcelas = 1 },
    };

    // ---------- documentos para devolução ----------
    public static readonly List<DocumentoDevolucaoNFe> DocumentosDevolucao = new()
    {
        new() { Tipo = "Saída", Numero = "000.015.892", Serie = "1", Emissao = "09/09/2026", Chave = "35260912345678000190550010000158921000158925", Contraparte = DSaoBento(), Itens = new() { Cat(2, 2) } },
        new() { Tipo = "Saída", Numero = "000.015.887", Serie = "1", Emissao = "05/09/2026", Chave = "35260912345678000190550010000158871000158879", Contraparte = DRotaSul(),
                Itens = new() { Cat(4, 2, i => { i.BaseFcpSt = 0; i.AliquotaFcpSt = 0; i.ValorFcpSt = 0; }), Cat(5, 3) } },
        new() { Tipo = "Entrada", Numero = "000.088.421", Serie = "3", Emissao = "03/09/2026", Chave = "35260998765432000110550030000884211000884219",
                Contraparte = D("Distribuidora Industrial Paulista Ltda", "98.765.432/0001-10", "492.118.220.114", "fiscal@dipindustrial.com.br", "(11) 4555-8810", "13201-005", "Avenida Nove de Julho", "1880", "Bloco B", "Anhangabaú", "Jundiaí"),
                Itens = new() { Cat(0, 30, i => { i.ValorUnitario = 38; i.Cfop = "1403"; i.Cst = "10"; i.BaseFcp = 1140; i.AliquotaFcp = 2; i.ValorFcp = 22.8m; }), Cat(1, 20, i => { i.ValorUnitario = 49; i.Cfop = "1102"; }) } },
        new() { Tipo = "Entrada", Numero = "000.041.776", Serie = "2", Emissao = "29/08/2026", Chave = "35260811223344000199550020000417761000417768",
                Contraparte = D("Eletro Peças Brasil S.A.", "11.223.344/0001-99", "244.990.771.118", "nfe@eletropecas.com.br", "(19) 3777-4400", "13080-650", "Rodovia Dom Pedro I", "5000", "Galpão 12", "Jardim Santa Genebra", "Campinas"),
                Itens = new() { Cat(2, 10, i => i.ValorUnitario = 350), Cat(3, 15, i => i.ValorUnitario = 71) } },
    };

    // ---------- operações em memória ----------
    public static NFeItem? Obter(string id) => Notas.FirstOrDefault(n => n.Id == id);

    public static string ProximoNumero()
    {
        var proximo = Notas.Max(n => int.Parse(n.Numero.Replace(".", ""))) + 1;
        var d = proximo.ToString("000000000");
        return $"{d[..3]}.{d[3..6]}.{d[6..]}";
    }

    public static NFeItem NovaNFe() => new()
    {
        Numero = ProximoNumero(),
        Emissao = DateTime.Now,
        Destinatario = new DestinatarioNFe { Uf = "SP" }
    };

    public static void Salvar(NFeItem nota)
    {
        var indice = Notas.FindIndex(n => n.Id == nota.Id);
        if (indice >= 0) Notas[indice] = nota;
        else Notas.Insert(0, nota);
    }

    public static NFeItem SalvarRascunho(NFeItem nota)
    {
        var copia = nota.Clonar();
        copia.Status = StatusNFe.Rascunho;
        Salvar(copia);
        return copia;
    }

    public static NFeItem Processar(NFeItem nota, AcaoFiscal acao)
    {
        var copia = nota.Clonar();
        if (acao == AcaoFiscal.Espelho)
        {
            copia.Status = StatusNFe.Assinada;
        }
        else
        {
            var d = copia.Numero.Replace(".", "");
            copia.Status = StatusNFe.Autorizada;
            copia.Chave = $"35260912345678000190550010{d}1000{d[^4..]}0";
            copia.Protocolo = "135260001998472";
        }
        Salvar(copia);
        return copia;
    }

    public static void Excluir(string id) => Notas.RemoveAll(n => n.Id == id);

    public static void Inutilizar(string id, string justificativa)
    {
        var nota = Obter(id);
        if (nota is null) return;
        nota.Status = StatusNFe.Inutilizada;
        nota.Protocolo = "135260001998599";
        nota.InformacoesComplementares = $"Numeração inutilizada. Justificativa: {justificativa}";
    }
}