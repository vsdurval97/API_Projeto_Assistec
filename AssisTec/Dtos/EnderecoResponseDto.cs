using AssistenciaTecnica.Api.Models;

namespace AssistenciaTecnica.Api.Dtos;

public record EnderecoResponseDto(
    string Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Municipio,
    string? Uf,
    string? CodigoMunicipioIbge)
{
    // Retorna null quando o Cliente não tem endereço cadastrado — mantém
    // o mesmo padrão de "opcional de ponta a ponta" já usado no resto da
    // API, em vez de forçar um objeto vazio com todos os campos nulos.
    public static EnderecoResponseDto? FromEntity(Endereco? endereco) => endereco is null
        ? null
        : new EnderecoResponseDto(
            endereco.Cep, endereco.Logradouro, endereco.Numero, endereco.Complemento,
            endereco.Bairro, endereco.Municipio, endereco.Uf, endereco.CodigoMunicipioIbge);
}