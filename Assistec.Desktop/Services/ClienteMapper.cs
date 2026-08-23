using AssistenciaTecnica.Api.Dtos;
using AssistenciaTecnica.Api.Models;

namespace Assistec.Desktop.Services;

// Centraliza a conversão entre o que a API devolve (ClienteResponseDto /
// EnderecoResponseDto) e o que ela espera receber num PUT
// (AtualizarClienteDto / EnderecoDto). Existe porque o PUT da API
// substitui o Cliente inteiro — qualquer tela que edite só uma parte dos
// campos precisa reenviar o resto inalterado, e essa lógica não pode
// ficar duplicada em cada tela nova.
public static class ClienteMapper
{
    public static EnderecoDto? ParaEnderecoDto(EnderecoResponseDto? origem) => origem is null
        ? null
        : new EnderecoDto(
            origem.Cep, origem.Logradouro, origem.Numero,
            origem.Complemento, origem.Bairro, origem.Municipio, origem.Uf);

    public static AtualizarClienteDto ParaAtualizacaoPreservando(
        ClienteResponseDto original, string novoNome, string novoTelefone) => new(
            novoNome,
            novoTelefone,
            original.Documento,
            original.TipoPessoa,
            original.IndicadorInscricaoEstadual,
            original.InscricaoEstadual,
            original.Email,
            ParaEnderecoDto(original.Endereco));
}