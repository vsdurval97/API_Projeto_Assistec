namespace AssistenciaTecnica.Api.Models;

// Owned type (sem Id próprio): não existe fora do contexto de um
// Cliente, então não faz sentido ser uma entidade independente com
// identidade e tabela navegável por si só no EF Core.
public class Endereco
{
    public required string Cep { get; set; }

    // Preenchidos automaticamente a partir do Cep, 
    // com possibilidade de o atendente sobrescrever manualmente —
    // necessário porque cidades com CEP único para todo o município (ex:
    // Estância/SE) não têm Logradouro/Bairro granulares para devolver.
    public string? Logradouro { get; set; }

    // Nunca vem de nenhuma API de CEP, em nenhuma cidade — é sempre
    // digitação manual do atendente.
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }

    // Município, UF e código IBGE são garantidos pela faixa do CEP em
    // si (diferente de Logradouro/Bairro, que dependem de granularidade
    // de rua) — por isso ficam confiáveis mesmo em CEP genérico.
    public string? Municipio { get; set; }
    public string? Uf { get; set; }
    public string? CodigoMunicipioIbge { get; set; }

    // Default já preenchido: na prática, 100% dos clientes de uma oficina
    // local serão do Brasil — evita repetir o mesmo valor em todo cadastro.
    public string CodigoPais { get; set; } = "1058";
    public string Pais { get; set; } = "Brasil";
}