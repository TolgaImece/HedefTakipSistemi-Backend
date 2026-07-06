using GoalService.Entities;
using Microsoft.EntityFrameworkCore;

namespace GoalService.Data;

public class GoalDbContext : DbContext
{
    public GoalDbContext(DbContextOptions<GoalDbContext> options) : base(options) { }

    public DbSet<GoalCategory>   GoalCategories   => Set<GoalCategory>();
    public DbSet<GoalTemplate>   GoalTemplates    => Set<GoalTemplate>();
    public DbSet<GoalAssignment> GoalAssignments  => Set<GoalAssignment>();
    public DbSet<GoalComment>    GoalComments     => Set<GoalComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GoalCategory>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<GoalTemplate>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(1000);
            e.Property(x => x.GoalType).HasMaxLength(20).IsRequired();
            e.HasOne(x => x.GoalCategory)
             .WithMany(x => x.GoalTemplates)
             .HasForeignKey(x => x.GoalCategoryId);
        });

        modelBuilder.Entity<GoalAssignment>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.HasOne(x => x.GoalTemplate)
             .WithMany(x => x.GoalAssignments)
             .HasForeignKey(x => x.GoalTemplateId);
        });

        modelBuilder.Entity<GoalComment>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Content).HasMaxLength(2000).IsRequired();
            e.HasOne(x => x.GoalAssignment)
             .WithMany(x => x.GoalComments)
             .HasForeignKey(x => x.GoalAssignmentId);
        });
    }
}
