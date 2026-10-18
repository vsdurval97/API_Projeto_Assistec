namespace Assistec.Desktop.Components.Pages.EstoquePages.NotaEntrada;

public record ItemXmlExtraido(string CodigoXml, string DescricaoXml, string Ncm, string Cfop, string UnidadeXml, double QuantidadeXml, double ValorUnitarioXml);

public record XmlImportadoInfo(
    string NumeroNfe, string Serie, string Chave, string Modelo, DateTime Emissao,
    string FornecedorNome, string FornecedorFantasia, string FornecedorCnpj, string FornecedorIe,
    string FornecedorCep, string FornecedorLogradouro, string FornecedorNumero, string FornecedorBairro,
    string FornecedorMunicipio, string FornecedorUf,
    double ValorTotalProdutos, double ValorTotalNota, List<ItemXmlExtraido> Itens);

public static class XmlImportadoMockData
{
    // Dados extraídos do XML de exemplo (NF-e 992346 — Plotag Sistemas e Suprimentos Ltda).
    // Em produção, essa extração será feita pelo backend ao ler o arquivo real.
    public static readonly XmlImportadoInfo Exemplo = new(
        "992346", "1", "35150300822602000124550010009923461099234656", "55 – NF-e",
        new DateTime(2015, 3, 27),
        "Plotag Sistemas e Suprimentos Ltda", "Plotag - Localhost", "00.822.602/0001-24", "114.489.114.119",
        "01127-010", "Rua Solon", "558", "Bom Retiro", "São Paulo", "SP",
        689.91, 689.91,
        new List<ItemXmlExtraido>
        {
            new("B17025056", "PAPEL MAXPLOT- 170MX250MX56GRS 3\"", "48025599", "5101", "RL", 1, 138.30),
            new("1070100752", "PAPEL MAXPLOT- 1070X100MX75GRS 2\"", "48025599", "5101", "RL", 1, 48.91),
            new("B17025056", "PAPEL MAXPLOT- 170MX250MX56GRS 3\"", "48025599", "5101", "RL", 1, 138.30),
            new("B17040056", "PAPEL MAXPLOT - 1.700X400MX 56 GRS 3\"", "48025599", "5101", "RL", 1, 214.57),
            new("B18525056", "PAPEL MAXPLOT-1.85MX250MX56GRS 3\"", "48025599", "5101", "RL", 1, 149.83),
        });
}