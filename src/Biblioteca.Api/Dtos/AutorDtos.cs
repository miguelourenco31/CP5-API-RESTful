using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Api.Dtos;

public class AutorRequest
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 120 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(60, ErrorMessage = "A nacionalidade deve ter no máximo 60 caracteres.")]
    public string? Nacionalidade { get; set; }

    public DateOnly? DataNascimento { get; set; }
}

public record AutorResponse(int Id, string Nome, string? Nacionalidade, DateOnly? DataNascimento);
