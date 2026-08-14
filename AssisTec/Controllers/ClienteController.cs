using AssistenciaTecnica.Api.Data;
using AssistenciaTecnica.Api.Dtos;
using AssistenciaTecnica.Api.Models;
using AssistenciaTecnica.Api.Services;
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
    private readonly ICepLocalizadorService _cepLocalizador;

    public ClienteController(
        AppDbContext context,
        ILogger<ClienteController> logger,
        ICepLocalizadorService cepLocalizador)
    {
        _context = context;
        _logger = logger;
        _cepLocalizador = cepLocalizador;
    }

    // POST: api/cliente
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ClienteResponseDto>> CriarCliente([FromBody] CriarClienteDto dto, CancellationToken ct = default)
    {
        var cliente = new Cliente
        {
            Nome = dto.Nome.Trim(),
            Telefone = dto.Telefone.Trim(),
            Documento = dto.Documento?.Trim(),
            TipoPessoa = dto.TipoPessoa,
            IndicadorInscricaoEstadual = dto.IndicadorInscricaoEstadual,
            InscricaoEstadual = dto.InscricaoEstadual?.Trim(),
            Email = dto.Email?.Trim(),
            Endereco = await ResolverEnderecoAsync(dto.Endereco, ct)
        };

        try
        {
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync(ct);
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
    public async Task<ActionResult<ClienteResponseDto>> AtualizarCliente(int id, [FromBody] AtualizarClienteDto dto, CancellationToken ct = default)
    {
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id, ct);

        if (cliente is null)
        {
            return NotFound(new { mensagem = $"Cliente com Id {id} não encontrado." });
        }

        // Mapeamento manual explícito — sem automappers
        cliente.Nome = dto.Nome.Trim();
        cliente.Telefone = dto.Telefone.Trim();
        cliente.Documento = dto.Documento?.Trim();
        cliente.TipoPessoa = dto.TipoPessoa;
        cliente.IndicadorInscricaoEstadual = dto.IndicadorInscricaoEstadual;
        cliente.InscricaoEstadual = dto.InscricaoEstadual?.Trim();
        cliente.Email = dto.Email?.Trim();
        cliente.Endereco = await ResolverEnderecoAsync(dto.Endereco, ct);

        try
        {
            await _context.SaveChangesAsync(ct);
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

    // Resolve o endereço campo a campo, nunca tudo-ou-nada: Município, UF
    // e código IBGE são garantidos pela faixa do CEP quando ele existe,
    // mas Logradouro/Bairro podem vir vazios em cidades com CEP único
    // para todo o município (ex: Estância/SE) — nesse caso, o que o
    // atendente digitou manualmente é preservado em vez de sobrescrito
    // por um valor vazio vindo da API externa. CodigoMunicipioIbge nunca
    // vem do usuário, só da consulta — é o único campo sem fallback manual.
    private async Task<Endereco?> ResolverEnderecoAsync(EnderecoDto? enderecoDto, CancellationToken ct)
    {
        if (enderecoDto is null)
        {
            return null;
        }

        // Falha na consulta (CEP inexistente, API fora do ar, timeout) não
        // bloqueia o cadastro — resultado nulo aqui apenas significa que
        // nenhum campo será complementado automaticamente, o endereço é
        // salvo com exatamente o que o atendente digitou.
        var resultado = await _cepLocalizador.BuscarPorCepAsync(enderecoDto.Cep, ct);

        return new Endereco
        {
            Cep = enderecoDto.Cep,
            Numero = enderecoDto.Numero,       // nunca vem da API, sempre manual
            Complemento = enderecoDto.Complemento, // idem
            Logradouro = string.IsNullOrWhiteSpace(resultado?.Logradouro) ? enderecoDto.Logradouro : resultado.Logradouro,
            Bairro = string.IsNullOrWhiteSpace(resultado?.Bairro) ? enderecoDto.Bairro : resultado.Bairro,
            Municipio = resultado?.Localidade ?? enderecoDto.Municipio,
            Uf = resultado?.Uf ?? enderecoDto.Uf,
            CodigoMunicipioIbge = resultado?.Ibge
        };
    }
}