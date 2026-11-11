using Microsoft.AspNetCore.Mvc;
using OficinaApi.Models;
using OficinaApi.Services;

namespace OficinaApi.Controllers;

[ApiController]
[Route("api/v1/veiculos")]
[Produces("application/json")]
public class VeiculosController : ControllerBase
{
    private readonly VeiculoService _service;

    public VeiculosController(VeiculoService service)
    {
        _service = service;
    }

    /// <summary>Lista todos os veículos cadastrados na oficina.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<Veiculo>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarTodos()
    {
        var veiculos = await _service.ListarTodosAsync();
        return Ok(veiculos);
    }

    /// <summary>Busca um veículo específico pelo seu ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Veiculo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(string id)
    {
        var veiculo = await _service.ObterPorIdAsync(id);
        if (veiculo is null)
            return NotFound(new { mensagem = $"Veículo com ID '{id}' não encontrado." });

        return Ok(veiculo);
    }

    /// <summary>Cadastra um novo veículo (valida placa duplicada).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Veiculo), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] Veiculo veiculo)
    {
        try
        {
            var criado = await _service.CriarAsync(veiculo);
            return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    /// <summary>Atualiza os dados de um veículo existente.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(string id, [FromBody] Veiculo veiculo)
    {
        try
        {
            var atualizado = await _service.AtualizarAsync(id, veiculo);
            if (!atualizado)
                return NotFound(new { mensagem = $"Veículo com ID '{id}' não encontrado." });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    /// <summary>Remove um veículo pelo seu ID.</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(string id)
    {
        var removido = await _service.RemoverAsync(id);
        if (!removido)
            return NotFound(new { mensagem = $"Veículo com ID '{id}' não encontrado." });

        return NoContent();
    }
}