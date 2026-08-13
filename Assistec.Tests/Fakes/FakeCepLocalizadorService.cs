// AssisTec.Tests/Fakes/FakeCepLocalizadorService.cs
using AssistenciaTecnica.Api.Dtos;
using AssistenciaTecnica.Api.Services;

namespace AssisTec.Tests.Fakes;

// Substitui ICepLocalizadorService nos testes de integração, evitando
// dependência de rede real (mesmo cuidado já tomado com InMemory/SQLite
// local em toda a suíte). Devolve respostas determinísticas para dois
// CEPs de teste conhecidos, cobrindo os dois cenários reais do domínio:
// endereço granular (cidade grande) e CEP genérico (cidade pequena, como
// Estância/SE). Qualquer outro CEP simula "não encontrado".
public sealed class FakeCepLocalizadorService : ICepLocalizadorService
{
    public const string CepComEnderecoCompleto = "49040490";
    public const string CepGenerico = "49200000";

    public Task<EnderecoViaCepDto?> BuscarPorCepAsync(string? cep, CancellationToken ct = default)
    {
        var cepNormalizado = cep?.Replace("-", "");

        EnderecoViaCepDto? resultado = cepNormalizado switch
        {
            CepComEnderecoCompleto => new EnderecoViaCepDto(
                Cep: "49040-490", Logradouro: "Rua Simeão Sobral", Bairro: "Suíssa",
                Localidade: "Aracaju", Uf: "SE", Ibge: "2800308"),

            CepGenerico => new EnderecoViaCepDto(
                Cep: "49200-000", Logradouro: "", Bairro: "",
                Localidade: "Estância", Uf: "SE", Ibge: "2802908"),

            _ => null
        };

        return Task.FromResult(resultado);
    }
}