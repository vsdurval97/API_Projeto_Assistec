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

    public DateTime DataAbertura { get; private set; } = DateTime.UtcNow;

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

      public DateTime UltimaModificacaoUtc { get; set; } = DateTime.UtcNow;

    public void AtualizarStatus(StatusOrdemServico novoStatus)
    {
        switch (novoStatus)
        {

            case StatusOrdemServico.Pronto:
                DataConclusao = DateTime.UtcNow;
                break;

            case StatusOrdemServico.Entregue:
                if (DataConclusao is null)
                    DataConclusao = DateTime.UtcNow;

                DataEntrega = DateTime.UtcNow;
                break;
        }

        Status = novoStatus;
    }
}