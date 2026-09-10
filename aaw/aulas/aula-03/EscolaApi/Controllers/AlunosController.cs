using EscolaApi.Data;
using EscolaApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EscolaApi.Controllers;

[ApiController]
[Route("api/v1/alunos")]
public class AlunosController : ControllerBase
{
    private readonly AppDbContext db;
    public AlunosController(AppDbContext db) { this.db = db; }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        if (page < 1 || size < 1 || size > 50)
            return Problem(
                title: "Parâmetros de paginação inválidos",
                detail: "page >= 1 e 1 <= size <= 50.",
                statusCode: StatusCodes.Status400BadRequest);

        var total = await db.Alunos.CountAsync();
        var alunos = await db.Alunos
            .OrderBy(a => a.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(a => new { a.Id, a.Nome, a.Curso })
            .ToListAsync();

        return Ok(new
        {
            page,
            size,
            totalItens = total,
            totalPaginas = (int)Math.Ceiling(total / (double)size),
            itens = alunos,
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var aluno = await db.Alunos.Include(a => a.Matriculas)
                                   .FirstOrDefaultAsync(a => a.Id == id);
        if (aluno is null) return NotFound();

        return Ok(aluno);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var aluno = await db.Alunos.FindAsync(id);
        if (aluno is null) return NotFound();

        db.Alunos.Remove(aluno);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id}/matriculas")]
    public async Task<IActionResult> GetMatriculas(int id)
    {
        var existe = await db.Alunos.AnyAsync(a => a.Id == id);
        if (!existe) return NotFound();

        var matriculas = await db.Matriculas.Where(m => m.AlunoId == id).ToListAsync();
        return Ok(matriculas);
    }

    [HttpGet("{id}/matriculas/{matriculaId}")]
    public async Task<IActionResult> GetMatricula(int id, int matriculaId)
    {
        var matricula = await db.Matriculas
            .FirstOrDefaultAsync(m => m.Id == matriculaId && m.AlunoId == id);
        if (matricula is null) return NotFound();

        return Ok(matricula);
    }
}