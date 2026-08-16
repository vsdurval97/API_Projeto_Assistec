using System.Text.Json;
using System.Text.RegularExpressions;
using AssistenciaTecnica.Api.Dtos;
namespace AssistenciaTecnica.Api.Services;

public interface ICepLocalizadorService
{
    // Retorna null em QUALQUER cenário de falha — CEP inexistente, CEP
    // mal formatado, erro de rede, timeout, resposta malformada. Nunca
    // lança exceção: o preenchimento automático de endereço é um bônus
    // para o atendente, nunca pode bloquear o cadastro de um cliente.
    Task<EnderecoViaCepDto?> BuscarPorCepAsync(string? cep, CancellationToken ct = default);
}