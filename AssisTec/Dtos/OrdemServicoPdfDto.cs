using AssistenciaTecnica.Api.Helpers;
using AssistenciaTecnica.Api.Models;

namespace AssistenciaTecnica.Api.Dtos;

// View model dedicado exclusivamente ao QuestPDF — separado de
// OrdemServicoResponseDto porque os dois têm propósitos diferentes: o
// DTO de resposta HTTP expõe dados brutos/tipados para um consumidor de
// API decidir como exibir; este DTO já entrega tudo formatado como
// string pronta para desenho, porque o motor de PDF não deveria conhecer
// regra de formatação nem lidar com CultureInfo — só desenhar o que recebe.
public sealed record OrdemServicoPdfDto(
    int Id,
    string ClienteNome,
    string ClienteTelefoneFormatado,
    string ClienteDocumentoFormatado,
    string DataAberturaFormatada,
    string DataConclusaoFormatada,
    string DataEntregaFormatada,
    string TipoEquipamento,
    string Marca,
    string Modelo,
    string DefeitoRelatado,
    string Status,
    string ValorMaoDeObraFormatado,
    string ValorPecasFormatado,
    string ValorTotalFormatado)
{
    public static OrdemServicoPdfDto FromEntity(OrdemServico ordem)
    {
        // Cliente nulo nunca deveria chegar aqui em uso normal (o
        // controller sempre carrega via Include), mas essa checagem existe
        // porque um recibo sem nome de cliente é um documento inválido —
        // melhor falhar cedo e explícito do que gerar um PDF com campo vazio.
        ArgumentNullException.ThrowIfNull(ordem.Cliente);

        // Valor negativo não deveria existir no banco (os DTOs de entrada
        // já bloqueiam isso na criação da OS), mas esta camada não confia
        // cegamente no dado persistido — um registro corrompido não pode
        // virar um recibo impresso com "-R$ 50,00" na mão do cliente.
        if (ordem.ValorMaoDeObra < 0 || ordem.ValorPecas < 0)
        {
            throw new InvalidOperationException(
                $"Ordem de Serviço {ordem.Id} possui valores negativos e não pode ser impressa.");
        }

        return new OrdemServicoPdfDto(
            Id: ordem.Id,
            ClienteNome: ordem.Cliente.Nome,
            ClienteTelefoneFormatado: FormatadorDados.FormatarTelefone(ordem.Cliente.Telefone),
            ClienteDocumentoFormatado: FormatadorDados.FormatarCpfCnpj(ordem.Cliente.Documento),
            DataAberturaFormatada: FormatadorDados.FormatarData(ordem.DataAbertura),
            DataConclusaoFormatada: FormatadorDados.FormatarDataOpcional(ordem.DataConclusao),
            DataEntregaFormatada: FormatadorDados.FormatarDataOpcional(ordem.DataEntrega),
            TipoEquipamento: ordem.TipoEquipamento.ToString(),
            Marca: ordem.Marca,
            Modelo: ordem.Modelo,
            DefeitoRelatado: ordem.DefeitoRelatado,
            Status: ordem.Status.ToString(),
            ValorMaoDeObraFormatado: FormatadorDados.FormatarMoeda(ordem.ValorMaoDeObra),
            ValorPecasFormatado: FormatadorDados.FormatarMoeda(ordem.ValorPecas),
            ValorTotalFormatado: FormatadorDados.FormatarMoeda(ordem.ValorTotal));
    }
}