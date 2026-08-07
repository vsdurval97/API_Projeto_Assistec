namespace AssistenciaTecnica.Api.Models;

public enum TipoEquipamento
{
    Computador,
    Notebook,
    Impressora,
    Outros
}

public enum StatusOrdemServico
{
    Recebido,
    EmAnalise,
    Pronto,
    Entregue
}

public class OrdemServico
{
    public int Id { get; set; }
    public DateTime DataAbertura { get; private set; } = DateTime.Now;
    public TipoEquipamento TipoEquipamento { get; set; }
    public required string Marca { get; set; }
    public required string Modelo { get; set; }
    public required string DefeitoRelatado { get; set; }
    public StatusOrdemServico Status { get; private set; } = StatusOrdemServico.Recebido;
    public decimal ValorMaoDeObra { get; set; }
    public decimal ValorPecas { get; set; }

    public decimal ValorTotal => ValorMaoDeObra + ValorPecas;
    public DateTime? DataConclusao { get; private set; }
    public DateTime? DataEntrega { get; private set; }

    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }


    public void AtualizarStatus(StatusOrdemServico novoStatus)
    {
        switch (novoStatus)
        {
            case StatusOrdemServico.Recebido:
                DataAbertura = DateTime.Now;
                break;

            case StatusOrdemServico.Pronto:
                DataConclusao = DateTime.Now;
                break;

            case StatusOrdemServico.Entregue:
                if (DataConclusao is null)
                    DataConclusao = DateTime.Now;

                DataEntrega = DateTime.Now;
                break;
        }

        Status = novoStatus;
    }
}