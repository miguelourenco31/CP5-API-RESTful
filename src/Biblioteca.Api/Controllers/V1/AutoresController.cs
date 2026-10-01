using Asp.Versioning;
using Biblioteca.Api.Data;
using Biblioteca.Api.Dtos;
using Biblioteca.Api.Mappings;
using Biblioteca.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class AutoresController(AppDbContext context) : ControllerBase
{
    /// <summary>Lista os autores. Aceita filtro opcional por nome (?nome=).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AutorResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AutorResponse>>> GetAll([FromQuery] string? nome, CancellationToken ct)
    {
        var query = context.Autores.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(nome))
            query = query.Where(a => EF.Functions.Like(a.Nome, $"%{nome}%"));

        var autores = await query.OrderBy(a => a.Nome).ToListAsync(ct);
        return Ok(autores.Select(a => a.ToResponse()));
    }

    /// <summary>Busca um autor pelo id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AutorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AutorResponse>> GetById(int id, CancellationToken ct)
    {
        var autor = await context.Autores.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);
        if (autor is null) return AutorNaoEncontrado(id);

        return Ok(autor.ToResponse());
    }

    /// <summary>Lista os livros de um autor.</summary>
    [HttpGet("{id:int}/livros")]
    [ProducesResponseType(typeof(IEnumerable<LivroResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<LivroResponse>>> GetLivros(int id, CancellationToken ct)
    {
        var existe = await context.Autores.AnyAsync(a => a.Id == id, ct);
        if (!existe) return AutorNaoEncontrado(id);

        var livros = await context.Livros
            .AsNoTracking()
            .Include(l => l.Autor)
            .Where(l => l.AutorId == id)
            .OrderBy(l => l.Titulo)
            .ToListAsync(ct);

        return Ok(livros.Select(l => l.ToResponse()));
    }

    /// <summary>Cadastra um novo autor.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AutorResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AutorResponse>> Create(AutorRequest request, CancellationToken ct)
    {
        var autor = new Autor
        {
            Nome = request.Nome.Trim(),
            Nacionalidade = request.Nacionalidade?.Trim(),
            DataNascimento = request.DataNascimento
        };

        context.Autores.Add(autor);
        await context.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = autor.Id, version = "1.0" }, autor.ToResponse());
    }

    /// <summary>Atualiza um autor existente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, AutorRequest request, CancellationToken ct)
    {
        var autor = await context.Autores.FindAsync([id], ct);
        if (autor is null) return AutorNaoEncontrado(id);

        autor.Nome = request.Nome.Trim();
        autor.Nacionalidade = request.Nacionalidade?.Trim();
        autor.DataNascimento = request.DataNascimento;

        await context.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Remove um autor. Só é possível se ele não tiver livros cadastrados.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var autor = await context.Autores.FindAsync([id], ct);
        if (autor is null) return AutorNaoEncontrado(id);

        if (await context.Livros.AnyAsync(l => l.AutorId == id, ct))
        {
            return Problem(
                title: "Autor possui livros",
                detail: "Não é possível excluir um autor que ainda possui livros cadastrados. Remova os livros antes.",
                statusCode: StatusCodes.Status409Conflict);
        }

        context.Autores.Remove(autor);
        await context.SaveChangesAsync(ct);
        return NoContent();
    }

    private ObjectResult AutorNaoEncontrado(int id) =>
        Problem(
            title: "Autor não encontrado",
            detail: $"Não existe autor com o id {id}.",
            statusCode: StatusCodes.Status404NotFound);
}
