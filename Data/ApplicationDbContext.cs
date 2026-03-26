using Microsoft.EntityFrameworkCore;
using RotinaXP.API.Models;

namespace RotinaXP.API.Data;

/// <summary>
/// Contexto de banco de dados para a aplicação RotinaXP.
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Inicializa uma nova instância da classe ApplicationDbContext.
    /// </summary>
    /// <param name="options">Opções de configuração do DbContext.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// DbSet para gerenciar usuários.
    /// </summary>
    public DbSet<Usuario> Usuarios { get; set; } = null!;

    /// <summary>
    /// DbSet para gerenciar tarefas.
    /// </summary>
    public DbSet<Tarefa> Tarefas { get; set; } = null!;
}
