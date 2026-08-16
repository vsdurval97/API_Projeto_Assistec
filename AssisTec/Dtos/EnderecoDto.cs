using System.ComponentModel.DataAnnotations;

namespace AssistenciaTecnica.Api.Dtos;

// Cep é o único campo obrigatório: é o dado que habilita o preenchimento
// automático do restante (Parte 4). Os demais são opcionais porque o
// atendente pode digitá-los manualmente quando o CEP não retornar
// granularidade suficiente (ex: CEP único cobrindo todo o município,
// caso real de Estância/SE) ou quando a consulta externa falhar.
public record EnderecoDto(
    [Required(ErrorMessage = "O CEP é obrigatório quando um endereço é informado.")]
    [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "CEP deve estar no formato 00000-000 ou 00000000.")]
    string Cep,

    [StringLength(200)] string? Logradouro = null,
    [StringLength(20)] string? Numero = null,
    [StringLength(100)] string? Complemento = null,
    [StringLength(100)] string? Bairro = null,
    [StringLength(100)] string? Municipio = null,

    // CodigoMunicipioIbge propositalmente NÃO existe aqui: é sempre
    // resolvido pelo servidor a partir do Cep, nunca aceito como entrada
    // do cliente da API — evita que alguém informe um código IBGE
    // divergente do endereço real.
    [StringLength(2, MinimumLength = 2)] string? Uf = null
);