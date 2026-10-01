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
public class LivrosController(AppDbContext context) : ControllerBase
{
    /// <summary>Lista os livros. Filtros opcionais: ?titulo= e ?autorId=.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LivroResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LivroResponse>>> GetAll(
        [FromQuery] string? titulo, [FromQuery] int? autorId, CancellationToken ct)
    {
        var query = context.Livros.AsNoTracking().Include(l => l.Autor).AsQueryable();

        if (!string.IsNullOrWhiteSpace(titulo))
            query = query.Where(l => EF.Functions.Like(l.Titulo, $"%{titulo}%"));

        if (autorId.HasValue)
            query = query.Where(l => l.AutorId == autorId.Value);

        var livros = await query.OrderBy(l => l.Titulo).ToListAsync(ct);
        return Ok(livros.Select(l => l.ToResponse()));
    }

    /// <summary>Busca um livro pelo id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(LivroResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LivroResponse>> GetById(int id, CancellationToken ct)
    {
        var livro = await context.Livros.AsNoTracking().Include(l => l.Autor).FirstOrDefaultAsync(l => l.Id == id, ct);
        if (livro is null) return LivroNaoEncontrado(id);

        return Ok(livro.ToResponse());
    }

    /// <summary>Cadastra um novo livro vinculado a um autor existente.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(LivroResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LivroResponse>> Create(LivroRequest request, CancellationToken ct)
    {
        var autor = await context.Autores.FindAsync([request.AutorId], ct);
        if (autor is null) return AutorInvalido(request.AutorId);

        var isbn = request.Isbn.Trim();
        if (await context.Livros.AnyAsync(l => l.Isbn == isbn, ct)) return IsbnDuplicado(isbn);

        var livro = new Livro
        {
            Titulo = request.Titulo.Trim(),
            Isbn = isbn,
            AnoPublicacao = request.AnoPublicacao,
            Genero = request.Genero?.Trim(),
            AutorId = autor.Id,
            Autor = autor
        };

        context.Livros.Add(livro);
        await context.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = livro.Id, version = "1.0" }, livro.ToResponse());
    }

    /// <summary>Atualiza um livro existente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, LivroRequest request, CancellationToken ct)
    {
        var livro = await context.Livros.FindAsync([id], ct);
        if (livro is null) return LivroNaoEncontrado(id);

        if (!await context.Autores.AnyAsync(a => a.Id == request.AutorId, ct))
            return AutorInvalido(request.AutorId);

        var isbn = request.Isbn.Trim();
        if (await context.Livros.AnyAsync(l => l.Isbn == isbn && l.Id != id, ct))
            return IsbnDuplicado(isbn);

        livro.Titulo = request.Titulo.Trim();
        livro.Isbn = isbn;
        livro.AnoPublicacao = request.AnoPublicacao;
        livro.Genero = request.Genero?.Trim();
        livro.AutorId = request.AutorId;

        await context.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Remove um livro.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var livro = await context.Livros.FindAsync([id], ct);
        if (livro is null) return LivroNaoEncontrado(id);

        context.Livros.Remove(livro);
        await context.SaveChangesAsync(ct);
        return NoContent();
    }

    private ObjectResult LivroNaoEncontrado(int id) =>
        Problem(
            title: "Livro não encontrado",
            detail: $"Não existe livro com o id {id}.",
            statusCode: StatusCodes.Status404NotFound);

    private ObjectResult AutorInvalido(int autorId) =>
        Problem(
            title: "Autor inválido",
            detail: $"O autor com id {autorId} não existe. Cadastre o autor antes de cadastrar o livro.",
            statusCode: StatusCodes.Status400BadRequest);

    private ObjectResult IsbnDuplicado(string isbn) =>
        Problem(
            title: "ISBN já cadastrado",
            detail: $"Já existe um livro cadastrado com o ISBN {isbn}.",
            statusCode: StatusCodes.Status409Conflict);
}
