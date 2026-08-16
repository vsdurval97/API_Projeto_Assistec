using System.Text.Json;
using System.Text.RegularExpressions;
using AssistenciaTecnica.Api.Dtos;
using Microsoft.Extensions.Logging;

namespace AssistenciaTecnica.Api.Services;

public sealed partial class CepLocalizadorService(HttpClient httpClient, ILogger<CepLocalizadorService> logger)
    : ICepLocalizadorService
{
    // Validado antes de qualquer chamada de rede: um CEP com formato
    // obviamente errado não precisa gastar uma requisição HTTP para
    // descobrir isso — mais rápido para o atendente e mais gentil com o
    // serviço externo.
    [GeneratedRegex(@"^\d{8}$")]
    private static partial Regex CepValidoRegex();

    public async Task<EnderecoViaCepDto?> BuscarPorCepAsync(string? cep, CancellationToken ct = default)
    {
        var cepNormalizado = NormalizarCep(cep);
        if (cepNormalizado is null)
        {
            return null;
        }

        try
        {
            using var response = await httpClient.GetAsync($"{cepNormalizado}/json/", ct);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("ViaCEP retornou {StatusCode} para o CEP {Cep}.", response.StatusCode, cepNormalizado);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            return InterpretarResposta(json, cepNormalizado);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            // As três exceções capturadas cobrem, respectivamente: falha
            // de rede/DNS, timeout do HttpClient, e resposta que não é um
            // JSON válido. Qualquer uma delas é "a API externa falhou de
            // algum jeito", nunca motivo para derrubar o cadastro do cliente.
            logger.LogWarning(ex, "Falha ao consultar ViaCEP para o CEP {Cep}.", cepNormalizado);
            return null;
        }
    }

    private static string? NormalizarCep(string? cep)
    {
        if (string.IsNullOrWhiteSpace(cep))
        {
            return null;
        }

        var apenasDigitos = new string(cep.Where(char.IsDigit).ToArray());
        return CepValidoRegex().IsMatch(apenasDigitos) ? apenasDigitos : null;
    }

    private EnderecoViaCepDto? InterpretarResposta(string json, string cepConsultado)
    {
        using var documento = JsonDocument.Parse(json);
        var raiz = documento.RootElement;

        // Particularidade do ViaCEP: CEP inexistente não retorna 404 HTTP,
        // retorna 200 OK com {"erro": true} no corpo — precisa ser checado
        // explicitamente, um parsing ingênuo baseado só no status code
        // passaria batido por esse caso.
        if (raiz.TryGetProperty("erro", out var erro) && erro.ValueKind == JsonValueKind.True)
        {
            logger.LogInformation("CEP {Cep} não encontrado no ViaCEP.", cepConsultado);
            return null;
        }

        return new EnderecoViaCepDto(
            Cep: LerCampo(raiz, "cep"),
            Logradouro: LerCampo(raiz, "logradouro"),
            Bairro: LerCampo(raiz, "bairro"),
            Localidade: LerCampo(raiz, "localidade"),
            Uf: LerCampo(raiz, "uf"),
            Ibge: LerCampo(raiz, "ibge"));
    }

    private static string LerCampo(JsonElement raiz, string nome)
        // GetString() pode retornar null tecnicamente, mas o ViaCEP sempre
        // inclui esses campos como string (mesmo que vazia, no caso de CEP
        // genérico) — ?? "" é só uma rede de segurança contra um contrato
        // de API que mudasse inesperadamente, não um caminho esperado.
        => raiz.TryGetProperty(nome, out var valor) ? valor.GetString() ?? "" : "";
}