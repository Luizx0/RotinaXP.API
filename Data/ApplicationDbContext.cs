using Microsoft.EntityFrameworkCore;
using RotinaXP.API.Models;
namespace RotinaXP.API.Data;
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<Tarefa> Tarefas { get; set; } = null!;
}
