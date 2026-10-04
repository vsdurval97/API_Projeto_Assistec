namespace Assistec.Desktop.Components.Pages.EstoquePages.Produtos;

public enum OrigemProduto
{
    Nacional, EstrangeiraImportacaoDireta, EstrangeiraMercadoInterno,
    NacionalConteudoEstrangeiroAcima40, NacionalProducaoBasica,
    NacionalConteudoEstrangeiroAte40, EstrangeiraImportacaoDiretaSemSimilar,
    EstrangeiraMercadoInternoSemSimilar, NacionalImportacaoInsumos
}

public enum ModalidadeBC { MargemValorAgregado, Pauta, PrecoTabelado, ValorDaOperacao }

public enum UnidadeMedida { UN, KG, CX, PC, MT, LT, M2, M3, PR, JG, TN, G, RL }

public enum StatusProduto { Ativo, Inativo, Descontinuado }

public record ProdutoItem(
    string Codigo, string CodigoInterno, string CodigoBarras, string CodigoFornecedor,
    string Descricao, string DescricaoComplementar, string Marca, string Modelo, string Categoria, StatusProduto Status,

    // Classificação Fiscal
    string Ncm, string Cest, string CfopPadrao, OrigemProduto Origem, string ExTipi,

    // Unidades
    UnidadeMedida UnidadeComercial, UnidadeMedida UnidadeTributavel, double FatorConversao,

    // Peso e Dimensões
    double PesoLiquido, double PesoBruto, double Largura, double Altura, double Profundidade,

    // ICMS
    string CstIcms, string? Csosn, ModalidadeBC ModalidadeBC, double AliquotaIcms, double ReducaoBc, double MvaSt, double AliquotaIcmsSt,

    // PIS / COFINS
    string CstPis, double AliquotaPis, string CstCofins, double AliquotaCofins,

    // IPI
    string CstIpi, double AliquotaIpi,

    // IBS / CBS / IS
    string CstIbs, double AliquotaIbsEstadual, double AliquotaIbsMunicipal, double ReducaoIbs, string DiferencialIbs,
    string CstCbs, double AliquotaCbs, double ReducaoCbs, double CreditoPresumidoCbs,
    bool IncideIs, string? CstIs, double AliquotaIs, string BaseCalculoIs, string? CategoriaIs,

    // Preços e Estoque
    double PrecoCusto, double PrecoVenda,
    int EstoqueAtual, int EstoqueMinimo, int EstoqueMaximo, string Localizacao,

    // Controle
    DateTime DataCadastro, DateTime UltimaAtualizacao, string FornecedorPrincipal)
{
    public double MargemLucro => PrecoCusto <= 0 ? 0 : Math.Round((PrecoVenda - PrecoCusto) / PrecoCusto * 100, 1);

    public string OrigemLabel => Origem switch
    {
        OrigemProduto.Nacional => "0 – Nacional",
        OrigemProduto.EstrangeiraImportacaoDireta => "1 – Estrangeira (importação direta)",
        OrigemProduto.EstrangeiraMercadoInterno => "2 – Estrangeira (mercado interno)",
        OrigemProduto.NacionalConteudoEstrangeiroAcima40 => "3 – Nacional c/ >40% cont. estrangeiro",
        OrigemProduto.NacionalProducaoBasica => "4 – Nacional produção básica",
        OrigemProduto.NacionalConteudoEstrangeiroAte40 => "5 – Nacional c/ ≤40% cont. estrangeiro",
        OrigemProduto.EstrangeiraImportacaoDiretaSemSimilar => "6 – Estrangeira (importação direta, sem similar)",
        OrigemProduto.EstrangeiraMercadoInternoSemSimilar => "7 – Estrangeira (mercado interno, sem similar)",
        _ => "8 – Nacional c/ importação de insumos"
    };

    public string OrigemCodigo => OrigemLabel.Split('–')[0].Trim();

    public string StatusLabel => Status.ToString();

    public string EstoqueStatus => EstoqueAtual == 0 ? "Zero" : EstoqueAtual < EstoqueMinimo ? "Baixo" : "Ok";

    private static string FormatarMoeda(double valor) => $"R$ {valor:N2}".Replace(",", "#").Replace(".", ",").Replace("#", ".");

    public string PrecoCustoFormatado => FormatarMoeda(PrecoCusto);
    public string PrecoVendaFormatado => FormatarMoeda(PrecoVenda);
}

public static class ProdutoMockData
{
    public static readonly string[] Categorias =
    {
        "Fixação", "Lubrificantes", "Ferramentas Manuais", "Material Elétrico", "EPI",
        "Rolamentos", "Tubulação", "Abrasivos", "Pneumática", "Hidráulica"
    };

    public static readonly List<ProdutoItem> Produtos = new()
    {
        new("PR0001", "PRD-001", "7891000315507", "FRN-A4501",
            "Parafuso Sextavado M8x50 Zincado", "Aço SAE 1020, cabeça sextavada, rosca métrica M8, comprimento 50mm, zincado eletrolítico",
            "Fischer", "M8x50", "Fixação", StatusProduto.Ativo,
            "73181590", "", "5102", OrigemProduto.Nacional, "",
            UnidadeMedida.CX, UnidadeMedida.UN, 100,
            0.480, 0.520, 15, 8, 8,
            "00", null, ModalidadeBC.ValorDaOperacao, 12, 0, 0, 0,
            "01", 0.65, "01", 3,
            "99", 0,
            "01", 13.3, 2.1, 0, "Não aplicável",
            "01", 8.8, 0, 0,
            false, null, 0, "Valor da Operação", null,
            18.90, 34.50,
            142, 50, 500, "B-04-02",
            new DateTime(2025, 1, 10), new DateTime(2026, 9, 2), "Ferrotec Distribuição Ltda"),

        new("PR0002", "PRD-002", "7896095300011", "FRN-B0092",
            "Óleo Lubrificante ISO VG 46", "Óleo mineral para sistemas hidráulicos e lubrificação geral, viscosidade ISO VG 46, balde 20L",
            "Petrobras", "Lubrax", "Lubrificantes", StatusProduto.Ativo,
            "27101980", "2300500", "5102", OrigemProduto.Nacional, "",
            UnidadeMedida.UN, UnidadeMedida.LT, 20,
            17.200, 18.500, 30, 42, 30,
            "10", null, ModalidadeBC.MargemValorAgregado, 12, 0, 35.5, 17,
            "01", 0.65, "01", 3,
            "99", 0,
            "01", 13.3, 2.1, 0, "Não aplicável",
            "01", 8.8, 0, 0,
            false, null, 0, "Valor da Operação", null,
            189.00, 295.00,
            18, 20, 100, "C-01-01",
            new DateTime(2025, 1, 15), new DateTime(2026, 9, 5), "Distribuidora Norte Ltda"),

        new("PR0003", "PRD-003", "7891234567890", "FRN-C2201",
            "Chave de Fenda Phillips PH2 x 150mm", "Cabo bimatéria ergonômico, haste cromo-vanádio temperada, ponta magnética PH2",
            "Tramontina", "Pro", "Ferramentas Manuais", StatusProduto.Ativo,
            "82059000", "", "5102", OrigemProduto.Nacional, "",
            UnidadeMedida.UN, UnidadeMedida.UN, 1,
            0.095, 0.120, 3, 3, 20,
            "00", null, ModalidadeBC.ValorDaOperacao, 12, 0, 0, 0,
            "01", 0.65, "01", 3,
            "00", 10,
            "01", 13.3, 2.1, 0, "Não aplicável",
            "01", 8.8, 0, 0,
            false, null, 0, "Valor da Operação", null,
            8.40, 18.90,
            7, 30, 200, "A-02-04",
            new DateTime(2025, 1, 20), new DateTime(2026, 9, 1), "Ferragens São Paulo ME"),

        new("PR0004", "PRD-004", "7893500910063", "FRN-A4501",
            "Cabo Elétrico Flexível 2,5mm² Azul", "Rolo 100m, condutor de cobre nu têmpera mole, isolação PVC, 750V, ABNT NBR 6812",
            "Prysmian", "Afumex", "Material Elétrico", StatusProduto.Ativo,
            "85443000", "", "5102", OrigemProduto.Nacional, "",
            UnidadeMedida.RL, UnidadeMedida.MT, 100,
            2.400, 2.600, 25, 25, 10,
            "00", null, ModalidadeBC.ValorDaOperacao, 12, 0, 0, 0,
            "01", 0.65, "01", 3,
            "99", 0,
            "02", 10.64, 1.68, 20, "Não aplicável",
            "02", 7.04, 20, 0,
            false, null, 0, "Valor da Operação", null,
            142.00, 210.00,
            34, 20, 150, "D-03-01",
            new DateTime(2025, 1, 25), new DateTime(2026, 9, 4), "Ferrotec Distribuição Ltda"),

        new("PR0005", "PRD-005", "7891150006021", "FRN-D1100",
            "Luva de Segurança Látex CA 10005", "Luva de borracha látex natural, resistente a produtos químicos diluídos, cano longo, par",
            "3M", "L-30", "EPI", StatusProduto.Ativo,
            "39261090", "", "5102", OrigemProduto.EstrangeiraImportacaoDireta, "",
            UnidadeMedida.PR, UnidadeMedida.PR, 1,
            0.180, 0.200, 12, 28, 5,
            "00", null, ModalidadeBC.ValorDaOperacao, 12, 0, 0, 0,
            "01", 0.65, "01", 3,
            "99", 0,
            "04", 0, 0, 100, "Não aplicável",
            "04", 0, 100, 0,
            false, null, 0, "Valor da Operação", null,
            12.50, 24.90,
            0, 50, 300, "E-01-03",
            new DateTime(2025, 2, 1), new DateTime(2026, 8, 20), "EPI Sul Distribuidora Ltda"),

        new("PR0006", "PRD-006", "7891007717013", "FRN-B0092",
            "Rolamento Rígido de Esferas 6205-2RS", "Rolamento de esferas d=25mm D=52mm B=15mm, blindado 2 lados, graxa, vedações borracha",
            "SKF", "6205-2RS1", "Rolamentos", StatusProduto.Ativo,
            "84821010", "", "5102", OrigemProduto.EstrangeiraImportacaoDireta, "",
            UnidadeMedida.UN, UnidadeMedida.UN, 1,
            0.130, 0.160, 7, 7, 3,
            "00", null, ModalidadeBC.ValorDaOperacao, 12, 0, 0, 0,
            "01", 0.65, "01", 3,
            "00", 5,
            "01", 13.3, 2.1, 0, "Não aplicável",
            "01", 8.8, 0, 0,
            false, null, 0, "Valor da Operação", null,
            28.00, 52.00,
            63, 20, 200, "B-02-06",
            new DateTime(2025, 2, 5), new DateTime(2026, 9, 6), "Distribuidora Norte Ltda"),

        new("PR0007", "PRD-007", "", "FRN-C2201",
            "Tubo de Aço Carbono Redondo 1\" Sch40", "Tubo redondo de aço carbono ASTM A53 Grau B, 1 polegada nominal, Sch40, comprimento 6m",
            "Vallourec", "Sch40 1\"", "Tubulação", StatusProduto.Inativo,
            "73043900", "", "5102", OrigemProduto.Nacional, "",
            UnidadeMedida.UN, UnidadeMedida.MT, 6,
            8.600, 9.000, 4, 4, 600,
            "00", null, ModalidadeBC.ValorDaOperacao, 12, 0, 0, 0,
            "01", 0.65, "01", 3,
            "99", 0,
            "01", 13.3, 2.1, 0, "Não aplicável",
            "01", 8.8, 0, 0,
            false, null, 0, "Valor da Operação", null,
            68.00, 105.00,
            12, 10, 80, "G-01-01",
            new DateTime(2025, 2, 10), new DateTime(2026, 7, 15), "Ferragens São Paulo ME"),
    };
}