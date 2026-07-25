using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Calories).HasPrecision(7, 2);
        builder.Property(p => p.Protein).HasPrecision(7, 2);
        builder.Property(p => p.Fat).HasPrecision(7, 2);
        builder.Property(p => p.Carbs).HasPrecision(7, 2);
        builder.Property(p => p.Price).IsRequired().HasPrecision(10, 2);

        builder.Property(p => p.OwnCalories).HasPrecision(7, 2);
        builder.Property(p => p.OwnProtein).HasPrecision(7, 2);
        builder.Property(p => p.OwnFat).HasPrecision(7, 2);
        builder.Property(p => p.OwnCarbs).HasPrecision(7, 2);
        builder.Property(p => p.OwnPrice).HasPrecision(10, 2);

        builder.Property(p => p.ImageUrl).HasMaxLength(500);
        builder.Property(p => p.Path).HasMaxLength(500);

        // ── Relationships ──────────────────────────────────────────────────────
        builder.HasMany(p => p.Categories).WithMany().UsingEntity("ProductCategories");

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder.HasOne(p => p.ParentProduct)
            .WithMany(p => p.Children)
            .HasForeignKey(p => p.ParentProductId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // ── Indexes ────────────────────────────────────────────────────────────
        builder.HasIndex(p => p.IsSystem);
        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.ParentProductId);
        builder.HasIndex(p => p.Path);
    }
}