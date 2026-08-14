namespace AssistenciaTecnica.Api.Models;

public enum TipoPessoa
{
    Fisica,
    Juridica
}

// Espelha o campo indIEDest do leiaute da SEFAZ (grupo dest do XML da
// NF-e/NFC-e). Os nomes aqui são legíveis para uso interno do C#; a
// conversão para os códigos numéricos oficiais (1/2/9) fica a cargo da
// futura camada de emissão fiscal, não desta entidade.
public enum IndicadorInscricaoEstadual
{
    ContribuinteIcms,   // indIEDest = 1 — exige InscricaoEstadual preenchida
    ContribuinteIsento, // indIEDest = 2
    NaoContribuinte     // indIEDest = 9 — caso comum de pessoa física
}

public class Cliente
{
    public int Id { get; set; }
    public required string Nome { get; set; }
    public required string Telefone { get; set; }

    // Opcional por decisão consciente: nem todo atendimento de oficina
    // exige nota fiscal/documento formal do cliente. Tornar obrigatório
    // bloquearia o cadastro rápido no balcão por um dado que nem sempre
    // está disponível no momento do atendimento.
    public string? Documento { get; set; }

    // Default Fisica: a esmagadora maioria dos clientes de uma oficina de
    // manutenção de PC/impressora é pessoa física levando o próprio
    // equipamento — Juridica é o caso menos comum, não o padrão.
    public TipoPessoa TipoPessoa { get; set; } = TipoPessoa.Fisica;

    // Default NaoContribuinte pelo mesmo motivo: pessoa física comum não
    // tem Inscrição Estadual. Só passa a ser ContribuinteIcms quando o
    // cadastro for de fato uma empresa que compra para revenda.
    public IndicadorInscricaoEstadual IndicadorInscricaoEstadual { get; set; } = IndicadorInscricaoEstadual.NaoContribuinte;

    // Só é preenchida (e só faz sentido) quando IndicadorInscricaoEstadual
    // for ContribuinteIcms — a validação dessa dependência fica para a
    // camada de emissão fiscal futura, não para a entidade em si.
    public string? InscricaoEstadual { get; set; }

    public string? Email { get; set; }

    // Owned type opcional: nem todo cliente tem endereço cadastrado hoje,
    // e forçar isso agora contradiria a decisão de manter o cadastro
    // rápido no balcão. A obrigatoriedade real ("preciso do endereço para
    // EMITIR a nota") é regra da futura camada de emissão, não do cadastro.
    public Endereco? Endereco { get; set; }

    public ICollection<OrdemServico> OrdensServico { get; set; } = new List<OrdemServico>();
}