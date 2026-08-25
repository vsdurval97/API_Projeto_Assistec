using System.ComponentModel.DataAnnotations;
using AssistenciaTecnica.Api.Helpers;

namespace AssistenciaTecnica.Api.Dtos;

public sealed class DocumentoValidoAttribute : ValidationAttribute
{
    public DocumentoValidoAttribute() : base("Documento inválido: não corresponde a um CPF nem a um CNPJ válido.")
    {
    }

    public override bool IsValid(object? value)
    {
        if (value is not string documento || string.IsNullOrWhiteSpace(documento))
        {
            return true; // opcional — ausência não é erro deste atributo
        }

        return ValidadorDocumento.CpfValido(documento) || ValidadorDocumento.CnpjValido(documento);
    }
}