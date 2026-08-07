using AssistenciaTecnica.Api.Models;

namespace AssistenciaTecnica.Api.Dtos;

public record OrdemServicoResponseDto(
    int Id,
    DateTime DataAbertura,
    TipoEquipamento TipoEquipamento,
    string Marca,
    string Modelo,
    string DefeitoRelatado,
    StatusOrdemServico Status,
    decimal ValorMaoDeObra,
    decimal ValorPecas,
    DateTime? DataConclusao,
    DateTime? DataEntrega,
    int ClienteId,
    string? ClienteNome
)
{
      public decimal ValorTotal => ValorMaoDeObra + ValorPecas;

    public static OrdemServicoResponseDto FromEntity(OrdemServico o) => new(
        o.Id,
        o.DataAbertura,
        o.TipoEquipamento,
        o.Marca,
        o.Modelo,
        o.DefeitoRelatado,
        o.Status,
        o.ValorMaoDeObra,
        o.ValorPecas,
        o.DataConclusao,
        o.DataEntrega,
        o.ClienteId,
        o.Cliente?.Nome
    );
}