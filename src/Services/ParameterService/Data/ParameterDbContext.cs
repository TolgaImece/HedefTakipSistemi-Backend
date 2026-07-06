using Microsoft.EntityFrameworkCore;
using ParameterService.Entities;

namespace ParameterService.Data;

public class ParameterDbContext : DbContext
{
    public ParameterDbContext(DbContextOptions<ParameterDbContext> options) : base(options) { }

    public DbSet<Period> Periods => Set<Period>();
    public DbSet<Parameter> Parameters => Set<Parameter>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Period>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Type).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Parameter>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Key).HasMaxLength(100).IsRequired();
            e.Property(x => x.Value).HasMaxLength(500).IsRequired();
            e.Property(x => x.Category).HasMaxLength(50).IsRequired();
            e.Property(x => x.Description).HasMaxLength(250);
            e.HasIndex(x => x.Key).IsUnique();
        });
    }
}
