namespace AssistenciaTecnica.Api.Models;

public class Cliente
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string Telefone { get; set; }

    // Opcional por decisão consciente: nem todo atendimento de oficina
    // exige nota fiscal/documento formal do cliente. Tornar obrigatório
    // bloquearia o cadastro rápido no balcão por um dado que nem sempre
    // está disponível no momento do atendimento.
    public string? Documento { get; set; }

    public ICollection<OrdemServico> OrdensServico { get; set; } = new List<OrdemServico>();
}