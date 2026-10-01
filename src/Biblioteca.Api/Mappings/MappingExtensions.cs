using Biblioteca.Api.Dtos;
using Biblioteca.Api.Models;

namespace Biblioteca.Api.Mappings;

public static class MappingExtensions
{
    public static AutorResponse ToResponse(this Autor autor) =>
        new(autor.Id, autor.Nome, autor.Nacionalidade, autor.DataNascimento);

    public static LivroResponse ToResponse(this Livro livro) =>
        new(livro.Id, livro.Titulo, livro.Isbn, livro.AnoPublicacao, livro.Genero, livro.AutorId, livro.Autor?.Nome);
}
