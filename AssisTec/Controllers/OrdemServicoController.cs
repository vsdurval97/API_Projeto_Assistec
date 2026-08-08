using System.Globalization;
using System.Text;
using AssistenciaTecnica.Api.Data;
using AssistenciaTecnica.Api.Dtos;
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

    // Mapa de transições de status permitidas — evita regressão inconsistente
    // (ex: voltar de "Entregue" para "Recebido" sem passar pelo fluxo normal).
    private static readonly Dictionary<StatusOrdemServico, StatusOrdemServico[]> TransicoesPermitidas = new()
    {
        [StatusOrdemServico.Recebido] = [StatusOrdemServico.EmAnalise],
        [StatusOrdemServico.EmAnalise] = [StatusOrdemServico.Pronto, StatusOrdemServico.Recebido],
        [StatusOrdemServico.Pronto] = [StatusOrdemServico.Entregue, StatusOrdemServico.EmAnalise],
        [StatusOrdemServico.Entregue] = [] // status final, nenhuma transição permitida
    };

    // Remove acentos e normaliza para minúsculas, permitindo comparação de
    // nomes robusta a diferenças de maiúsculas/minúsculas e acentuação
    // (ex: "JOSE DA COSTA" e "José da Costa" devem ser tratados como iguais).
    private static string NormalizarNome(string texto)
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

    public OrdemServicoController(AppDbContext context, ILogger<OrdemServicoController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // POST: api/ordemservico
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrdemServicoResponseDto>> CriarOrdemServico([FromBody] CriarOrdemServicoDto dto)
    {
        // a) Nem ClienteId nem ClienteNome foram informados
        if (dto.ClienteId is null && string.IsNullOrWhiteSpace(dto.ClienteNome))
        {
            return BadRequest(new { mensagem = "É necessário informar o ID ou o Nome do cliente." });
        }

        int clienteIdResolvido;

        if (dto.ClienteId is not null)
        {
            // ClienteId tem prioridade quando informado explicitamente
            var clienteExiste = await _context.Clientes.AnyAsync(c => c.Id == dto.ClienteId.Value);
            if (!clienteExiste)
            {
                return NotFound(new { mensagem = $"Cliente com Id {dto.ClienteId.Value} não encontrado." });
            }

            clienteIdResolvido = dto.ClienteId.Value;
        }
        else
        {
            // b) e c) Busca por ClienteNome — comparação tolerante a maiúsculas/
            // minúsculas E a acentuação. O LOWER() nativo do SQLite (sem extensão
            // ICU) não normaliza diacríticos, então a comparação é feita em
            // memória. Aceitável para o volume de dados de uma oficina local.
            var nomeBuscadoNormalizado = NormalizarNome(dto.ClienteNome!.Trim());

            var todosOsClientes = await _context.Clientes
                .AsNoTracking()
                .Select(c => new ClienteResponseDto(c.Id, c.Nome, c.Telefone))
                .ToListAsync();

            var clientesEncontrados = todosOsClientes
                .Where(c => NormalizarNome(c.Nome) == nomeBuscadoNormalizado)
                .ToList();

            if (clientesEncontrados.Count == 0)
            {
                return NotFound(new { mensagem = $"Nenhum cliente encontrado com o nome '{dto.ClienteNome}'." });
            }

            if (clientesEncontrados.Count > 1)
                {
                    return BadRequest(new
                        {
                            mensagem = $"Foram encontrados {clientesEncontrados.Count} clientes com o nome '{dto.ClienteNome}'. " +
                            "Informe o ClienteId específico na requisição para prosseguir.",
                            clientesEncontrados
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
            _logger.LogError(ex, "Erro ao salvar Ordem de Serviço no banco.");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { mensagem = "Erro ao salvar a Ordem de Serviço. Tente novamente." });
        }

        // Recarrega com o Cliente incluído, para popular ClienteNome no DTO de resposta
        await _context.Entry(ordemServico).Reference(o => o.Cliente).LoadAsync();

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

    // Guarda contra KeyNotFoundException: se o Status persistido não estiver
    // mapeado (dado corrompido, edição manual no banco, ou enum novo sem
    // atualizar o dicionário), retorna 500 tratado em vez de exceção crua.
    if (!TransicoesPermitidas.TryGetValue(ordem.Status, out var transicoesValidas))
    {
        _logger.LogError(
            "Status '{Status}' da OS {Id} não está mapeado em TransicoesPermitidas.", ordem.Status, id);
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
        // Agora este catch é ALCANÇÁVEL de verdade: dispara quando duas
        // requisições concorrentes tentam alterar a mesma OS.
        _logger.LogError(ex, "Conflito de concorrência ao atualizar status da OS {Id}.", id);
        return Conflict(new { mensagem = "A Ordem de Serviço foi modificada por outra requisição. Recarregue os dados e tente novamente." });
    }

    return Ok(OrdemServicoResponseDto.FromEntity(ordem));
}
}