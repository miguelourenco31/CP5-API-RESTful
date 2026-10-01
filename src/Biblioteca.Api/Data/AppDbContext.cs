using Biblioteca.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Autor> Autores => Set<Autor>();
    public DbSet<Livro> Livros => Set<Livro>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Autor>(e =>
        {
            e.ToTable("Autores");
            e.HasKey(a => a.Id);
            e.Property(a => a.Nome).IsRequired().HasMaxLength(120);
            e.Property(a => a.Nacionalidade).HasMaxLength(60);

            e.HasData(
                new Autor { Id = 1, Nome = "Machado de Assis", Nacionalidade = "Brasileira", DataNascimento = new DateOnly(1839, 6, 21) },
                new Autor { Id = 2, Nome = "Clarice Lispector", Nacionalidade = "Brasileira", DataNascimento = new DateOnly(1920, 12, 10) });
        });

        modelBuilder.Entity<Livro>(e =>
        {
            e.ToTable("Livros");
            e.HasKey(l => l.Id);
            e.Property(l => l.Titulo).IsRequired().HasMaxLength(200);
            e.Property(l => l.Isbn).IsRequired().HasMaxLength(20);
            e.Property(l => l.Genero).HasMaxLength(60);
            e.HasIndex(l => l.Isbn).IsUnique();

            // Relacionamento 1:N (um autor possui vários livros).
            // Restrict impede apagar um autor que ainda tenha livros.
            e.HasOne(l => l.Autor)
             .WithMany(a => a.Livros)
             .HasForeignKey(l => l.AutorId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasData(
                new Livro { Id = 1, Titulo = "Dom Casmurro", Isbn = "978-85-0000-001-1", AnoPublicacao = 1899, Genero = "Romance", AutorId = 1 },
                new Livro { Id = 2, Titulo = "Memórias Póstumas de Brás Cubas", Isbn = "978-85-0000-002-8", AnoPublicacao = 1881, Genero = "Romance", AutorId = 1 },
                new Livro { Id = 3, Titulo = "A Hora da Estrela", Isbn = "978-85-0000-003-5", AnoPublicacao = 1977, Genero = "Romance", AutorId = 2 });
        });
    }
}
