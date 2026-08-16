namespace AssistenciaTecnica.Api.Dtos;

// Espelha só os campos do ViaCEP que interessam ao sistema — a API
// devolve mais campos (ddd, siafi, gia, etc.) que não têm uso aqui.
// JsonPropertyName não é necessário porque os nomes já batem em
// lowercase com o padrão de serialização do System.Text.Json.
public sealed record EnderecoViaCepDto(
    string Cep,
    string Logradouro,
    string Bairro,
    string Localidade,
    string Uf,
    string Ibge);