using System.Globalization;
using System.Text.RegularExpressions;

namespace AssistenciaTecnica.Api.Helpers;

// Centraliza toda formatação de exibição usada no recibo em PDF. Fica
// separado de OrdemServicoPdfDto porque são funções puras (entrada ->
// saída, sem estado, sem dependência de entidade) — o mesmo motivo que
// levou NormalizadorTexto a sair do controller: testável isoladamente,
// sem precisar montar um DTO ou uma OS inteira só para validar uma máscara.
public static class FormatadorDados
{
    // Sem CultureInfo explícita, o separador decimal e de milhar mudariam
    // conforme a cultura do sistema operacional onde a API rodar — um
    // recibo de oficina no Brasil não pode variar formato dependendo da
    // máquina. "pt-BR" é fixado deliberadamente, não é o padrão do host.
    private static readonly CultureInfo CulturaPtBr = new("pt-BR");

    public static string FormatarCpfCnpj(string? documento)
    {
        var apenasDigitos = ExtrairDigitos(documento);

        // Um documento ausente ou com quantidade de dígitos que não bate
        // com CPF nem CNPJ não é motivo para travar a geração do PDF — o
        // documento é informação secundária no recibo, então o valor
        // original (ou um placeholder) é exibido sem mascarar, em vez de
        // a função falhar e derrubar a geração inteira por um campo que
        // nunca foi validado como obrigatório na entrada.
        return apenasDigitos.Length switch
        {
            11 => $"{apenasDigitos[..3]}.{apenasDigitos[3..6]}.{apenasDigitos[6..9]}-{apenasDigitos[9..]}",
            14 => $"{apenasDigitos[..2]}.{apenasDigitos[2..5]}.{apenasDigitos[5..8]}/{apenasDigitos[8..12]}-{apenasDigitos[12..]}",
            _ => string.IsNullOrWhiteSpace(documento) ? "—" : documento
        };
    }

    public static string FormatarTelefone(string? telefone)
    {
        var apenasDigitos = ExtrairDigitos(telefone);

        // Celular (11 dígitos, com o 9º dígito) e fixo (10 dígitos) têm
        // máscaras diferentes porque são formatos de fato distintos no
        // Brasil, não uma variação estética — aplicar a máscara de celular
        // num fixo deixaria o número com posição de dígito errada.
        return apenasDigitos.Length switch
        {
            11 => $"({apenasDigitos[..2]}) {apenasDigitos[2..7]}-{apenasDigitos[7..]}",
            10 => $"({apenasDigitos[..2]}) {apenasDigitos[2..6]}-{apenasDigitos[6..]}",
            _ => string.IsNullOrWhiteSpace(telefone) ? "—" : telefone
        };
    }

    public static string FormatarMoeda(decimal valor)
        // "C2" já cobriria o padrão monetário, mas depende inteiramente da
        // CultureInfo passada — fixar "pt-BR" aqui é o que garante "R$" e
        // vírgula decimal independente de onde a API for hospedada.
        => valor.ToString("C2", CulturaPtBr);

    public static string FormatarData(DateTime data)
        => data.ToString("dd/MM/yyyy", CulturaPtBr);

    public static string FormatarDataOpcional(DateTime? data, string valorPadrao = "—")
        // Datas de conclusão/entrega ficam nulas por boa parte do ciclo de
        // vida da OS (uma OS "Recebido" não tem DataConclusao ainda) — o
        // recibo precisa mostrar um placeholder nesse caso, não uma
        // exceção nem uma string vazia que pareceria um bug de layout.
        => data.HasValue ? FormatarData(data.Value) : valorPadrao;

    private static string ExtrairDigitos(string? texto)
        // Aceita entrada já mascarada (ex: "123.456.789-00") ou crua (ex:
        // "12345678900") sem exigir que quem chama saiba qual formato o
        // cadastro salvou — normaliza antes de decidir CPF vs. CNPJ.
        => texto is null ? string.Empty : Regex.Replace(texto, @"\D", string.Empty);
}