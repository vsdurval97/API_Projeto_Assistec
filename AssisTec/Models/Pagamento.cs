namespace AssistenciaTecnica.Api.Models;

// Espelha os códigos oficiais de tPag do grupo de pagamento (YA) do leiaute
// da NF-e/NFC-e — os valores numéricos atribuídos aqui são os mesmos da
// tabela da SEFAZ, não uma sequência arbitrária. Isso permite obter o
// código de 2 dígitos exigido no XML só com ((int)Forma).ToString("D2"),
// sem precisar manter um dicionário de tradução separado.
// Fonte: Manual de Orientação do Contribuinte (MOC), grupo <detPag>/<tPag>.
public enum FormaPgto
{
    Dinheiro = 1,
    Cheque = 2,
    CartaoCredito = 3,
    CartaoDebito = 4,
    CreditoLoja = 5,
    ValeAlimentacao = 10,
    ValeRefeicao = 11,
    ValePresente = 12,
    ValeCombustivel = 13,
    BoletoBancario = 15,
    DepositoBancario = 16,
    PagamentoInstantaneoPix = 17,
    TransferenciaBancariaCarteiraDigital = 18,
    ProgramaFidelidadeCashbackCreditoVirtual = 19,
    SemPagamento = 90,
    Outros = 99
}

public class Pagamento
{
    public int Id { get; set; }
    public FormaPgto Forma { get; set; }
    public decimal Valor { get; set; }
    public string? Descricao { get; set; }

    public int OrdemServicoId { get; set; }
    public OrdemServico? OrdemServico { get; set; }
}