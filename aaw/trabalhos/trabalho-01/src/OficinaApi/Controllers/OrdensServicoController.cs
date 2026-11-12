using Microsoft.AspNetCore.Mvc;
using OficinaApi.Models;
using OficinaApi.Services;

namespace OficinaApi.Controllers;

[ApiController]
[Route("api/v1/ordens-servico")]
[Produces("application/json")]
public class OrdensServicoController : ControllerBase
{
    private readonly OrdemServicoService _service;

    public OrdensServicoController(OrdemServicoService service)
    {
        _service = service;
    }

    /// <summary>Lista todas as ordens de serviço (com filtro opcional por veículo ou status).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<OrdemServico>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarTodas([FromQuery] string? veiculoId, [FromQuery] string? status)
    {
        var ordens = await _service.ListarTodasAsync(veiculoId, status);
        return Ok(ordens);
    }

    /// <summary>Busca uma ordem de serviço específica pelo seu ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrdemServico), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(string id)
    {
        var os = await _service.ObterPorIdAsync(id);
        if (os is null)
            return NotFound(new { mensagem = $"Ordem de serviço com ID '{id}' não encontrada." });

        return Ok(os);
    }

    /// <summary>Abre uma nova ordem de serviço e calcula o valor total no servidor.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrdemServico), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Criar([FromBody] OrdemServico ordemServico)
    {
        try
        {
            var criada = await _service.CriarAsync(ordemServico);
            return CreatedAtAction(nameof(ObterPorId), new { id = criada.Id }, criada);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
    }

    /// <summary>Atualiza valores, descrição ou status de uma ordem de serviço aberta.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(string id, [FromBody] OrdemServico ordemServico)
    {
        try
        {
            var atualizada = await _service.AtualizarAsync(id, ordemServico);
            if (!atualizada)
                return NotFound(new { mensagem = $"Ordem de serviço com ID '{id}' não encontrada." });

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
    }

    /// <summary>Remove uma ordem de serviço pelo seu ID.</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(string id)
    {
        var removida = await _service.RemoverAsync(id);
        if (!removida)
            return NotFound(new { mensagem = $"Ordem de serviço com ID '{id}' não encontrada." });

        return NoContent();
    }
}