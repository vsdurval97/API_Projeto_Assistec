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

    // Imutável após a criação: já foi motivo de um bug real onde uma
    // regressão de status (EmAnalise -> Recebido) sobrescrevia essa data,
    // apagando o registro de quando o equipamento realmente chegou.
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

    // Token de concorrência otimista: sem ele, duas requisições concorrentes
    // alterando a mesma OS resultam em "o último que salvar, vence", sem
    // nenhum erro — o EF Core só detecta o conflito se esse valor divergir
    // entre o SELECT e o UPDATE.
    public DateTime UltimaModificacaoUtc { get; set; } = DateTime.UtcNow;

    // Fica na entidade, não no controller, porque descreve o mesmo ciclo de
    // vida que AtualizarStatus() implementa logo abaixo — separar "quais
    // transições existem" de "o que elas fazem" em arquivos diferentes é o
    // tipo de duplicação que dessincroniza quando só um lado é editado.
    private static readonly Dictionary<StatusOrdemServico, StatusOrdemServico[]> TransicoesPermitidas = new()
    {
        [StatusOrdemServico.Recebido] = [StatusOrdemServico.EmAnalise],
        [StatusOrdemServico.EmAnalise] = [StatusOrdemServico.Pronto, StatusOrdemServico.Recebido],
        [StatusOrdemServico.Pronto] = [StatusOrdemServico.Entregue, StatusOrdemServico.EmAnalise],
        [StatusOrdemServico.Entregue] = [] // estado final: nenhuma saída é válida
    };

    // TryGetValue em vez de indexador direto: um Status fora do mapa (dado
    // corrompido, edição manual no banco, ou um valor de enum novo que
    // esqueceram de mapear aqui) não deve virar uma exceção não tratada —
    // quem chama decide como responder a esse caso (o controller retorna
    // 500 tratado, por exemplo).
    public static bool TryObterTransicoesPermitidas(StatusOrdemServico origem, out StatusOrdemServico[] transicoesPermitidas)
    {
        if (TransicoesPermitidas.TryGetValue(origem, out var encontradas))
        {
            transicoesPermitidas = encontradas;
            return true;
        }

        // Garante um array vazio, nunca null — mesmo que o chamador ignore o
        // retorno bool e use o out diretamente, não deve haver NullReferenceException.
        transicoesPermitidas = Array.Empty<StatusOrdemServico>();
        return false;
    }

    public void AtualizarStatus(StatusOrdemServico novoStatus)
    {
        switch (novoStatus)
        {
            case StatusOrdemServico.Pronto:
                DataConclusao = DateTime.UtcNow;
                break;

            case StatusOrdemServico.Entregue:
                // A guarda existe porque Entregue é tecnicamente alcançável
                // sem passar por Pronto se alguém chamar este método
                // diretamente (fora da máquina de estados do controller) —
                // sem ela, DataConclusao ficaria nula numa OS já entregue.
                if (DataConclusao is null)
                    DataConclusao = DateTime.UtcNow;

                DataEntrega = DateTime.UtcNow;
                break;
        }

        Status = novoStatus;
    }
}