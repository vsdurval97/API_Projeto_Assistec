using AssistenciaTecnica.Api.Data;
using AssistenciaTecnica.Api.Dtos;
using AssistenciaTecnica.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssistenciaTecnica.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdemServicoController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<OrdemServicoController> _logger;

    // Mapa de transições de status permitidas — evita regressão inconsistente
    // (ex: voltar de "Entregue" para "Recebido" sem passar pelo fluxo normal).
    private static readonly Dictionary<StatusOrdemServico, StatusOrdemServico[]> TransicoesPermitidas = new()
    {
        [StatusOrdemServico.Recebido] = [StatusOrdemServico.EmAnalise],
        [StatusOrdemServico.EmAnalise] = [StatusOrdemServico.Pronto, StatusOrdemServico.Recebido],
        [StatusOrdemServico.Pronto] = [StatusOrdemServico.Entregue, StatusOrdemServico.EmAnalise],
        [StatusOrdemServico.Entregue] = [] // status final, nenhuma transição permitida
    };

    public OrdemServicoController(AppDbContext context, ILogger<OrdemServicoController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // POST: api/ordemservico
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrdemServicoResponseDto>> CriarOrdemServico([FromBody] CriarOrdemServicoDto dto)
    {
        var clienteExiste = await _context.Clientes.AnyAsync(c => c.Id == dto.ClienteId);
        if (!clienteExiste)
        {
            return NotFound(new { mensagem = $"Cliente com Id {dto.ClienteId} não encontrado." });
        }

        var ordemServico = new OrdemServico
        {
            TipoEquipamento = dto.TipoEquipamento,
            Marca = dto.Marca.Trim(),
            Modelo = dto.Modelo.Trim(),
            DefeitoRelatado = dto.DefeitoRelatado.Trim(),
            ValorMaoDeObra = dto.ValorMaoDeObra,
            ValorPecas = dto.ValorPecas,
            ClienteId = dto.ClienteId,
            
        };

        try
        {
            _context.OrdensServico.Add(ordemServico);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao salvar Ordem de Serviço no banco.");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { mensagem = "Erro ao salvar a Ordem de Serviço. Tente novamente." });
        }

        var response = OrdemServicoResponseDto.FromEntity(ordemServico);
        return CreatedAtAction(nameof(BuscarPorId), new { id = ordemServico.Id }, response);
    }

    // GET: api/ordemservico
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrdemServicoResponseDto>>> ListarTodas()
    {
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

        var transicoesValidas = TransicoesPermitidas[ordem.Status];
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
            _logger.LogError(ex, "Conflito de concorrência ao atualizar status da OS {Id}.", id);
            return Conflict(new { mensagem = "A Ordem de Serviço foi modificada por outra requisição. Tente novamente." });
        }

        return Ok(OrdemServicoResponseDto.FromEntity(ordem));
    }
}