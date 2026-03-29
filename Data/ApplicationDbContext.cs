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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.RowVersion)
            .HasDefaultValue(0L)
            .IsConcurrencyToken();

        modelBuilder.Entity<TaskItem>()
            .HasIndex(t => t.UserId);

        modelBuilder.Entity<Reward>()
            .HasIndex(r => r.UserId);

        modelBuilder.Entity<DailyProgress>()
            .Property(p => p.Date)
            .HasColumnType("date");

        modelBuilder.Entity<DailyProgress>()
            .HasIndex(p => p.UserId);

        modelBuilder.Entity<DailyProgress>()
            .HasIndex(p => new { p.UserId, p.Date })
            .IsUnique();
    }
}
