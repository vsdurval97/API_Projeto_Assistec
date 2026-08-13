using System.ComponentModel.DataAnnotations;
using AssistenciaTecnica.Api.Models;

namespace AssistenciaTecnica.Api.Dtos;

public record AtualizarClienteDto(
    [Required(ErrorMessage = "O nome do cliente é obrigatório.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 150 caracteres.")]
    string Nome,

    [Required(ErrorMessage = "O telefone do cliente é obrigatório.")]
    [StringLength(20, MinimumLength = 8, ErrorMessage = "O telefone deve ter entre 8 e 20 caracteres.")]
    string Telefone,

    string? Documento = null,
    TipoPessoa TipoPessoa = TipoPessoa.Fisica,
    IndicadorInscricaoEstadual IndicadorInscricaoEstadual = IndicadorInscricaoEstadual.NaoContribuinte,
    string? InscricaoEstadual = null,

    [EmailAddress(ErrorMessage = "E-mail em formato inválido.")]
    string? Email = null,

    EnderecoDto? Endereco = null
);