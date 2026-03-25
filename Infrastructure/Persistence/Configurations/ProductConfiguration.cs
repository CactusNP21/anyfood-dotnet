using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Calories)
            .HasPrecision(7, 2);

        builder.Property(p => p.Protein)
            .HasPrecision(7, 2);

        builder.Property(p => p.Fat)
            .HasPrecision(7, 2);

        builder.Property(p => p.Carbs)
            .HasPrecision(7, 2);

        builder.Property(p => p.ImageUrl)
            .HasMaxLength(500);

        builder.Property(p => p.Price)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(p => p.Price)
            .HasPrecision(10, 2);

        // ── Relationships ──────────────────────────────────────────────────────
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        // ── Indexes ────────────────────────────────────────────────────────────
        builder.HasIndex(p => p.IsSystem);
        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.CategoryId);
    }
}

