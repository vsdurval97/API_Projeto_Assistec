using System.ComponentModel.DataAnnotations;
using AssistenciaTecnica.Api.Models;

namespace AssistenciaTecnica.Api.Dtos;

public record AtualizarStatusDto(
    [Required, EnumDataType(typeof(StatusOrdemServico))]
    StatusOrdemServico Status
);