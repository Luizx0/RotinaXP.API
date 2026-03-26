using Microsoft.EntityFrameworkCore;
using RotinaXP.API.Models;
namespace RotinaXP.API.Data;
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<TaskItem> Tasks { get; set; } = null!;
    public DbSet<Reward> Rewards { get; set; } = null!;
    public DbSet<DailyProgress> DailyProgresses { get; set; } = null!;
}
