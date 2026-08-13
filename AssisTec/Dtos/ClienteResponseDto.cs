using AssistenciaTecnica.Api.Models;

namespace AssistenciaTecnica.Api.Dtos;

public record ClienteResponseDto(
    int Id,
    string Nome,
    string Telefone,

    // Todos com default: preserva compatibilidade com os pontos do código
    // que já constroem ClienteResponseDto de forma posicional e enxuta
    // (ex: a lista de candidatos em OrdemServicoController, quando um
    // nome de cliente é ambíguo) sem exigir que sejam atualizados agora.
    string? Documento = null,
    TipoPessoa TipoPessoa = TipoPessoa.Fisica,
    IndicadorInscricaoEstadual IndicadorInscricaoEstadual = IndicadorInscricaoEstadual.NaoContribuinte,
    string? InscricaoEstadual = null,
    string? Email = null,
    EnderecoResponseDto? Endereco = null)
{
    public static ClienteResponseDto FromEntity(Cliente c) => new(
        c.Id,
        c.Nome,
        c.Telefone,
        c.Documento,
        c.TipoPessoa,
        c.IndicadorInscricaoEstadual,
        c.InscricaoEstadual,
        c.Email,
        EnderecoResponseDto.FromEntity(c.Endereco));
}