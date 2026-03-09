using Lawllit.Data.Finance.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lawllit.Data.Finance;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.GoogleId);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.EmailConfirmationToken);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.PasswordResetToken);

        modelBuilder.Entity<Transaction>()
            .HasIndex(t => new { t.UserId, t.Date });

        modelBuilder.Entity<Category>()
            .HasIndex(c => c.UserId);
    }
}
