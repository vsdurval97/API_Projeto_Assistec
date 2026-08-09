using AssistenciaTecnica.Api.Data;
using AssistenciaTecnica.Api.Dtos;
using AssistenciaTecnica.Api.Helpers;
using AssistenciaTecnica.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssistenciaTecnica.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdemServicoController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<OrdemServicoController> _logger;

    public OrdemServicoController(AppDbContext context, ILogger<OrdemServicoController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Separado de ClienteResponseDto propositalmente: esse DTO é o contrato
    // público de resposta da API, e usá-lo também como estrutura interna de
    // busca faria uma mudança futura no formato de saída (ex: um campo só
    // relevante para exibição) se propagar silenciosamente para esta lógica
    // de negócio, sem nenhuma relação real entre as duas coisas.
    private sealed record ClienteEncontrado(int Id, string Nome, string Telefone);

    // POST: api/ordemservico
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrdemServicoResponseDto>> CriarOrdemServico([FromBody] CriarOrdemServicoDto dto)
    {
        // ClienteId e ClienteNome são mutuamente supletivos, não obrigatórios
        // ao mesmo tempo — reflete o fluxo real do técnico, que às vezes só
        // sabe o nome do cliente na hora de abrir a OS.
        if (dto.ClienteId is null && string.IsNullOrWhiteSpace(dto.ClienteNome))
        {
            return BadRequest(new { mensagem = "É necessário informar o ID ou o Nome do cliente." });
        }

        int clienteIdResolvido;

        if (dto.ClienteId is not null)
        {
            // ClienteId tem prioridade sobre ClienteNome quando os dois vêm
            // preenchidos: é a informação inequívoca, um nome pode ser ambíguo.
            var clienteExiste = await _context.Clientes.AnyAsync(c => c.Id == dto.ClienteId.Value);
            if (!clienteExiste)
            {
                return NotFound(new { mensagem = $"Cliente com Id {dto.ClienteId.Value} não encontrado." });
            }

            clienteIdResolvido = dto.ClienteId.Value;
        }
        else
        {
            // Carrega a tabela inteira em memória porque a normalização de
            // acento (ver NormalizadorTexto) não é traduzível para SQL do
            // SQLite sem extensão ICU. Aceitável na escala de uma oficina
            // local; se a base crescer, isso vira candidato a otimização
            // (coluna NomeNormalizado indexada).
            var nomeBuscadoNormalizado = NormalizadorTexto.RemoverAcentosEMinusculas(dto.ClienteNome!.Trim());

            var todosOsClientes = await _context.Clientes
                .AsNoTracking()
                .Select(c => new ClienteEncontrado(c.Id, c.Nome, c.Telefone))
                .ToListAsync();

            var clientesEncontrados = todosOsClientes
                .Where(c => NormalizadorTexto.RemoverAcentosEMinusculas(c.Nome) == nomeBuscadoNormalizado)
                .ToList();

            if (clientesEncontrados.Count == 0)
            {
                return NotFound(new { mensagem = $"Nenhum cliente encontrado com o nome '{dto.ClienteNome}'." });
            }

            if (clientesEncontrados.Count > 1)
            {
                // Nome ambíguo não é erro do sistema, é decisão que só o
                // consumidor da API pode tomar — por isso devolve a lista de
                // candidatos em vez de escolher um arbitrariamente (ex: o
                // primeiro cadastrado), o que esconderia a ambiguidade.
                var candidatos = clientesEncontrados
                    .Select(c => new ClienteResponseDto(c.Id, c.Nome, c.Telefone))
                    .ToList();

                return BadRequest(new
                {
                    mensagem = $"Foram encontrados {clientesEncontrados.Count} clientes com o nome '{dto.ClienteNome}'. " +
                                "Informe o ClienteId específico na requisição para prosseguir.",
                    clientesEncontrados = candidatos
                });
            }

            clienteIdResolvido = clientesEncontrados[0].Id;
        }

        var ordemServico = new OrdemServico
        {
            TipoEquipamento = dto.TipoEquipamento,
            Marca = dto.Marca.Trim(),
            Modelo = dto.Modelo.Trim(),
            DefeitoRelatado = dto.DefeitoRelatado.Trim(),
            ValorMaoDeObra = dto.ValorMaoDeObra,
            ValorPecas = dto.ValorPecas,
            ClienteId = clienteIdResolvido
        };

        try
        {
            _context.OrdensServico.Add(ordemServico);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // Mensagem genérica para o cliente da API, stack trace só no log —
            // evita vazar detalhe de schema/infra em uma resposta HTTP.
            _logger.LogError(ex, "Erro ao salvar Ordem de Serviço no banco.");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { mensagem = "Erro ao salvar a Ordem de Serviço. Tente novamente." });
        }

        // Necessário porque a entidade recém-inserida não traz o relacionamento
        // carregado em memória — sem isso, ClienteNome sairia nulo na resposta.
        await _context.Entry(ordemServico).Reference(o => o.Cliente).LoadAsync();

        var response = OrdemServicoResponseDto.FromEntity(ordemServico);
        return CreatedAtAction(nameof(BuscarPorId), new { id = ordemServico.Id }, response);
    }

    // GET: api/ordemservico
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrdemServicoResponseDto>>> ListarTodas()
    {
        // AsNoTracking: dado só será lido e devolvido, não há necessidade de
        // o EF Core pagar o custo de rastrear mudanças que nunca vão ocorrer.
        var ordens = await _context.OrdensServico
            .Include(o => o.Cliente)
            .AsNoTracking()
            .OrderByDescending(o => o.DataAbertura)
            .Select(o => OrdemServicoResponseDto.FromEntity(o))
            .ToListAsync();

        return Ok(ordens);
    }

    // GET: api/ordemservico/{id}
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrdemServicoResponseDto>> BuscarPorId(int id)
    {
        var ordem = await _context.OrdensServico
            .Include(o => o.Cliente)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);

        if (ordem is null)
        {
            return NotFound(new { mensagem = $"Ordem de Serviço com Id {id} não encontrada." });
        }

        return Ok(OrdemServicoResponseDto.FromEntity(ordem));
    }

    // PUT: api/ordemservico/{id}/status
    [HttpPut("{id:int}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrdemServicoResponseDto>> AtualizarStatus(int id, [FromBody] AtualizarStatusDto dto)
    {
        var ordem = await _context.OrdensServico
            .Include(o => o.Cliente)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (ordem is null)
        {
            return NotFound(new { mensagem = $"Ordem de Serviço com Id {id} não encontrada." });
        }

        if (ordem.Status == dto.Status)
        {
            return BadRequest(new { mensagem = $"A Ordem de Serviço já está com o status '{dto.Status}'." });
        }

        // A regra de "quais transições existem" mora na entidade (ver
        // OrdemServico.TryObterTransicoesPermitidas) — o controller só decide
        // o status HTTP para cada resultado, sem precisar conhecer o fluxo.
        if (!OrdemServico.TryObterTransicoesPermitidas(ordem.Status, out var transicoesValidas))
        {
            _logger.LogError(
                "Status '{Status}' da OS {Id} não está mapeado nas transições permitidas.", ordem.Status, id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { mensagem = "Estado da Ordem de Serviço inconsistente. Contate o suporte técnico." });
        }

        if (!transicoesValidas.Contains(dto.Status))
        {
            return BadRequest(new
            {
                mensagem = $"Transição de status inválida: não é possível ir de '{ordem.Status}' para '{dto.Status}'.",
                statusPermitidos = transicoesValidas
            });
        }

        ordem.AtualizarStatus(dto.Status);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Alcançável de verdade porque OrdemServico.UltimaModificacaoUtc é
            // um concurrency token real — dispara quando duas requisições
            // concorrentes tentam alterar a mesma OS entre o SELECT e o UPDATE.
            _logger.LogError(ex, "Conflito de concorrência ao atualizar status da OS {Id}.", id);
            return Conflict(new { mensagem = "A Ordem de Serviço foi modificada por outra requisição. Recarregue os dados e tente novamente." });
        }

        return Ok(OrdemServicoResponseDto.FromEntity(ordem));
    }
}