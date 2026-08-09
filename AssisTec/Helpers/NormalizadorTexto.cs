using System.Globalization;
using System.Text;

namespace AssistenciaTecnica.Api.Helpers;

// Extraído do controller porque não depende de HTTP nem de estado de
// requisição — é lógica de domínio pura (comparação de texto), então
// precisa ser testável sem instanciar um controller e reutilizável em
// qualquer outro lugar que compare nomes (ex: ClienteController, se um dia
// precisar impedir cadastro de clientes com nomes visualmente diferentes
// mas semanticamente iguais).
public static class NormalizadorTexto
{
    // SQLite não tem extensão ICU habilitada por padrão, então LOWER() no
    // SQL não normaliza acentos ("É" != "é" só é resolvido, "É" vs "E" não
    // é resolvido de jeito nenhum). Por isso essa normalização acontece em
    // memória, no C#, e não é delegada para a query.
    public static string RemoverAcentosEMinusculas(string texto)
    {
        var textoDecomposto = texto.Normalize(NormalizationForm.FormD);
        var semAcentos = new StringBuilder();

        foreach (var caractere in textoDecomposto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caractere) != UnicodeCategory.NonSpacingMark)
            {
                semAcentos.Append(caractere);
            }
        }

        return semAcentos.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
    }
}