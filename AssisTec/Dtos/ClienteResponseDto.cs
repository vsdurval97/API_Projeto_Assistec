using AssistenciaTecnica.Api.Models;

namespace AssistenciaTecnica.Api.Dtos;

public record ClienteResponseDto(
    int Id,
    string Nome,
    string Telefone
)
{
    public static ClienteResponseDto FromEntity(Cliente c) => new(
        c.Id,
        c.Nome,
        c.Telefone
    );
}