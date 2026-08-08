using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AssistenciaTecnica.Api.Models;

namespace AssistenciaTecnica.Api.Dtos;

public record AtualizarStatusDto(
    [property: JsonRequired]
    [Required, EnumDataType(typeof(StatusOrdemServico))]
    StatusOrdemServico Status
);