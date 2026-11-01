using BibliotecaApi.Data;
using BibliotecaApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Autores")]
public class AutoresController : ControllerBase
{
    private readonly BibliotecaRepository _repo;

    public AutoresController(BibliotecaRepository repo) => _repo = repo;

    /// <summary>
    /// Lista todos os autores cadastrados.
    /// </summary>
    /// <returns>Lista de autores com seus livros associados.</returns>
    /// <response code="200">Retorna a lista de autores.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<Autor>), StatusCodes.Status200OK)]
    public IActionResult ObterTodos()
    {
        return Ok(_repo.ObterAutores());
    }

    /// <summary>
    /// Obtém um autor específico pelo seu ID.
    /// </summary>
    /// <param name="id">ID único do autor.</param>
    /// <returns>Dados do autor encontrado.</returns>
    /// <response code="200">Autor encontrado com sucesso.</response>
    /// <response code="404">Autor não encontrado.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Autor), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult ObterPorId(int id)
    {
        var autor = _repo.ObterAutorPorId(id);
        if (autor == null)
            return NotFound(new { mensagem = $"Autor com ID {id} não encontrado." });
        return Ok(autor);
    }

    /// <summary>
    /// Cadastra um novo autor no sistema.
    /// </summary>
    /// <param name="autor">Dados do autor a ser criado.</param>
    /// <returns>Autor recém-criado.</returns>
    /// <response code="201">Autor criado com sucesso.</response>
    /// <response code="400">Dados inválidos enviados na requisição.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Autor), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Criar([FromBody] Autor autor)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var criado = _repo.CriarAutor(autor);
        return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
    }

    /// <summary>
    /// Atualiza os dados de um autor existente.
    /// </summary>
    /// <param name="id">ID do autor a atualizar.</param>
    /// <param name="autor">Novos dados do autor.</param>
    /// <response code="204">Autor atualizado com sucesso.</response>
    /// <response code="404">Autor não encontrado.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Atualizar(int id, [FromBody] Autor autor)
    {
        if (!_repo.AtualizarAutor(id, autor))
            return NotFound(new { mensagem = $"Autor com ID {id} não encontrado." });
        return NoContent();
    }

    /// <summary>
    /// Remove um autor do sistema.
    /// </summary>
    /// <param name="id">ID do autor a remover.</param>
    /// <response code="204">Autor removido com sucesso.</response>
    /// <response code="404">Autor não encontrado.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Remover(int id)
    {
        if (!_repo.RemoverAutor(id))
            return NotFound(new { mensagem = $"Autor com ID {id} não encontrado." });
        return NoContent();
    }
}