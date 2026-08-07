using AssistenciaTecnica.Api.Data;
using AssistenciaTecnica.Api.Dtos;
using AssistenciaTecnica.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssistenciaTecnica.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClienteController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ClienteController> _logger;

    public ClienteController(AppDbContext context, ILogger<ClienteController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // POST: api/cliente
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ClienteResponseDto>> CriarCliente([FromBody] CriarClienteDto dto)
    {
        var cliente = new Cliente
        {
            Nome = dto.Nome.Trim(),
            Telefone = dto.Telefone.Trim()
        };

        try
        {
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao salvar Cliente no banco.");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { mensagem = "Erro ao salvar o Cliente. Tente novamente." });
        }

        var response = ClienteResponseDto.FromEntity(cliente);
        return CreatedAtAction(nameof(BuscarPorId), new { id = cliente.Id }, response);
    }

    // GET: api/cliente
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteResponseDto>>> ListarTodos()
    {
        var clientes = await _context.Clientes
            .AsNoTracking()
            .OrderBy(c => c.Nome)
            .Select(c => ClienteResponseDto.FromEntity(c))
            .ToListAsync();

        return Ok(clientes);
    }

    // GET: api/cliente/{id}
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteResponseDto>> BuscarPorId(int id)
    {
        var cliente = await _context.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cliente is null)
        {
            return NotFound(new { mensagem = $"Cliente com Id {id} não encontrado." });
        }

        return Ok(ClienteResponseDto.FromEntity(cliente));
    }

    // PUT: api/cliente/{id}
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClienteResponseDto>> AtualizarCliente(int id, [FromBody] AtualizarClienteDto dto)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id);

        if (cliente is null)
        {
            return NotFound(new { mensagem = $"Cliente com Id {id} não encontrado." });
        }

        // Mapeamento manual explícito — sem automappers
        cliente.Nome = dto.Nome.Trim();
        cliente.Telefone = dto.Telefone.Trim();

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Conflito de concorrência ao atualizar Cliente {Id}.", id);
            return Conflict(new { mensagem = "O Cliente foi modificado por outra requisição. Tente novamente." });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao atualizar Cliente {Id} no banco.", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { mensagem = "Erro ao atualizar o Cliente. Tente novamente." });
        }

        return Ok(ClienteResponseDto.FromEntity(cliente));
    }
}