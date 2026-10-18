namespace Assistec.Desktop.Components.Pages.EstoquePages.Produtos;

public static class ProdutoOpcoesFiscais
{
    public static readonly string[] CstIcms = { "00", "10", "20", "30", "40", "41", "50", "51", "60", "70", "90" };
    public static readonly string[] Csosn = { "102", "103", "300", "400", "500", "900" };
    public static readonly string[] CstPisCofins = { "01", "02", "03", "04", "05", "06", "07", "08", "09", "49", "50", "70", "99" };
    public static readonly string[] CstIpi = { "00", "01", "02", "03", "04", "05", "49", "50", "51", "52", "53", "54", "55", "99" };

    public static readonly (string Codigo, string Label)[] CstIbsCbs =
    {
        ("01", "01 – Tributado integralmente"),
        ("02", "02 – Tributado com redução de alíquota"),
        ("03", "03 – Imune"),
        ("04", "04 – Isento"),
        ("05", "05 – Não incidência"),
        ("06", "06 – Suspensão"),
        ("07", "07 – Diferimento"),
        ("08", "08 – Tributado com crédito presumido"),
        ("09", "09 – Tributado e isento (operação mista)"),
        ("10", "10 – Regime específico / especial"),
        ("40", "40 – Não tributado (saída p/ exportação)"),
        ("99", "99 – Outras"),
    };

    public static readonly string[] CategoriasIs =
    {
        "Bebidas alcoólicas", "Bebidas açucaradas", "Tabaco e derivados",
        "Veículos automotores", "Embarcações e aeronaves", "Bens minerais (extração)",
        "Armas e munições", "Produtos prejudiciais à saúde"
    };

    public static readonly string[] DifalOpcoes =
    {
        "Não aplicável",
        "Operação interestadual – recolhe destino",
        "Operação interestadual – partilha transição",
        "Operação com consumidor final não contribuinte"
    };

    public static readonly string[] BaseCalculoIsOpcoes = { "Valor da Operação", "Unidade de Medida", "Peso" };
}