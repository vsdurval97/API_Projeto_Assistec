namespace Assistec.Desktop.Components.Pages.EstoquePages.Produtos;

public enum TipoMovimentacao { Entrada, Saida, Ajuste }

public record MovimentacaoEstoqueItem(
    string ProdutoCodigo, DateTime Data, TipoMovimentacao Tipo, int Quantidade,
    string Origem, string Usuario, int SaldoResultante)
{
    public string TipoLabel => Tipo switch
    {
        TipoMovimentacao.Entrada => "Entrada",
        TipoMovimentacao.Saida => "Saída",
        _ => "Ajuste"
    };

    public string QuantidadeFormatada => Tipo switch
    {
        TipoMovimentacao.Entrada => $"+{Quantidade}",
        TipoMovimentacao.Saida => $"-{Quantidade}",
        _ => Quantidade >= 0 ? $"+{Quantidade}" : Quantidade.ToString()
    };
}

public static class MovimentacaoEstoqueMockData
{
    public static readonly List<MovimentacaoEstoqueItem> Movimentacoes = new()
    {
        new("PR0001", new DateTime(2026, 8, 26), TipoMovimentacao.Entrada, 200, "Nota de Entrada — NF-e 45231", "Henrique Costa", 262),
        new("PR0001", new DateTime(2026, 8, 27), TipoMovimentacao.Saida, 8, "Venda — PV-0847", "Lucas Andrade", 254),
        new("PR0001", new DateTime(2026, 8, 30), TipoMovimentacao.Saida, 45, "Venda — PV-0860", "Lucas Andrade", 209),
        new("PR0001", new DateTime(2026, 9, 1), TipoMovimentacao.Ajuste, -5, "Ajuste de inventário — divergência de contagem", "Rafael Mendonça", 204),
        new("PR0001", new DateTime(2026, 9, 2), TipoMovimentacao.Saida, 62, "Venda — PV-0892", "Lucas Andrade", 142),
    };
}