using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AssistenciaTecnica.Api.Models;

namespace AssistenciaTecnica.Api.Dtos;

public record CriarOrdemServicoDto(
    [property: JsonRequired]
    [Required, EnumDataType(typeof(TipoEquipamento))]
    TipoEquipamento TipoEquipamento,

    [Required, StringLength(100, MinimumLength = 1)]
    string Marca,

    [Required, StringLength(100, MinimumLength = 1)]
    string Modelo,

    [Required, StringLength(500, MinimumLength = 3)]
    string DefeitoRelatado,

    [Range(0, double.MaxValue, ErrorMessage = "O valor da mão de obra não pode ser negativo.")]
    decimal ValorMaoDeObra,

    [Range(0, double.MaxValue, ErrorMessage = "O valor das peças não pode ser negativo.")]
    decimal ValorPecas,

    [Range(1, int.MaxValue, ErrorMessage = "ClienteId inválido.")]
    int? ClienteId = null,

    string? ClienteNome = null
);