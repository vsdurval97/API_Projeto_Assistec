using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AssistenciaTecnica.Api.Models;

namespace AssistenciaTecnica.Api.Dtos;

public record PagamentoDto(
    [property: JsonRequired]
    [Required, EnumDataType(typeof(FormaPgto))]
    FormaPgto Forma,

    [Range(0.01, double.MaxValue, ErrorMessage = "O valor do pagamento deve ser maior que zero.")]
    decimal Valor,

    // Espelha o xPag do XML fiscal: só relevante quando Forma = Outros.
    string? Descricao = null
);