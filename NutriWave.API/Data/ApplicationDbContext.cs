using Microsoft.EntityFrameworkCore;
using NutriWave.API.Models;

namespace NutriWave.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<UserInformation> Users { get; set; }
    public DbSet<Nutrient> Nutrients { get; set; }
    public DbSet<UserNutrientIntake> UserNutrientIntakes { get; set; }
    public DbSet<UserNutrientRequirement> UserNutrientRequirements { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserInformation>(entity =>
        {
            entity.HasKey(u => u.UserId);
            entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(u => u.MedicalReportUrl).HasMaxLength(500);

            entity.HasMany(u => u.NutrientIntakes)
                  .WithOne(ni => ni.User)
                  .HasForeignKey(ni => ni.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.NutrientRequirements)
                  .WithOne(nr => nr.User)
                  .HasForeignKey(nr => nr.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Nutrient>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Name).IsRequired().HasMaxLength(100);
            entity.Property(n => n.Unit).IsRequired().HasMaxLength(20);

            entity.HasMany(n => n.Intakes)
                  .WithOne(ni => ni.Nutrient)
                  .HasForeignKey(ni => ni.NutrientId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(n => n.Requirements)
                  .WithOne(nr => nr.Nutrient)
                  .HasForeignKey(nr => nr.NutrientId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserNutrientRequirement>(entity =>
        {
            entity.HasKey(nr => nr.Id);
            entity.Property(nr => nr.Quantity).IsRequired();

            entity.HasOne(nr => nr.User)
                  .WithMany(u => u.NutrientRequirements)
                  .HasForeignKey(nr => nr.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(nr => nr.Nutrient)
                  .WithMany(n => n.Requirements)
                  .HasForeignKey(nr => nr.NutrientId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserNutrientIntake>(entity =>
        {
            entity.HasKey(ni => ni.Id);
            entity.Property(ni => ni.Quantity).IsRequired();
            entity.Property(ni => ni.Date).IsRequired();

            entity.HasOne(ni => ni.User)
                  .WithMany(u => u.NutrientIntakes)
                  .HasForeignKey(ni => ni.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ni => ni.Nutrient)
                  .WithMany(n => n.Intakes)
                  .HasForeignKey(ni => ni.NutrientId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

