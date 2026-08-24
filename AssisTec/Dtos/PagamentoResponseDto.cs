using AssistenciaTecnica.Api.Models;

namespace AssistenciaTecnica.Api.Dtos;

public record PagamentoResponseDto(
    int Id,
    FormaPgto Forma,
    decimal Valor,
    string? Descricao
)
{
    public static PagamentoResponseDto FromEntity(Pagamento p) => new(p.Id, p.Forma, p.Valor, p.Descricao);
}