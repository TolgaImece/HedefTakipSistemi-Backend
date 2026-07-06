using AuditService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditService.Data;

public class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ServiceName).HasMaxLength(50).IsRequired();
            e.Property(x => x.Action).HasMaxLength(50).IsRequired();
            e.Property(x => x.EntityName).HasMaxLength(100);
            e.Property(x => x.EntityId).HasMaxLength(100);
            e.Property(x => x.UserId).HasMaxLength(100);
            e.Property(x => x.IpAddress).HasMaxLength(50);
            e.HasIndex(x => x.ServiceName);
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.CreatedTime);
        });
    }
}
