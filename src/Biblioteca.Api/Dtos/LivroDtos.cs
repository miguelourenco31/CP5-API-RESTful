using System.ComponentModel.DataAnnotations;

namespace Biblioteca.Api.Dtos;

public class LivroRequest
{
    [Required(ErrorMessage = "O título é obrigatório.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "O título deve ter no máximo 200 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "O ISBN é obrigatório.")]
    [StringLength(20, MinimumLength = 10, ErrorMessage = "O ISBN deve ter entre 10 e 20 caracteres.")]
    public string Isbn { get; set; } = string.Empty;

    [Range(1450, 2100, ErrorMessage = "O ano de publicação deve estar entre 1450 e 2100.")]
    public int AnoPublicacao { get; set; }

    [StringLength(60, ErrorMessage = "O gênero deve ter no máximo 60 caracteres.")]
    public string? Genero { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Informe um AutorId válido.")]
    public int AutorId { get; set; }
}

public record LivroResponse(int Id, string Titulo, string Isbn, int AnoPublicacao, string? Genero, int AutorId, string? AutorNome);
