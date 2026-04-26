using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ProductVersionConfiguration : IEntityTypeConfiguration<ProductVersion>
{
    public void Configure(EntityTypeBuilder<ProductVersion> builder)
    {
        builder.HasKey(pv => pv.Id);

        builder.Property(pv => pv.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(pv => pv.Calories).HasPrecision(7, 2);
        builder.Property(pv => pv.Protein).HasPrecision(7, 2);
        builder.Property(pv => pv.Fat).HasPrecision(7, 2);
        builder.Property(pv => pv.Carbs).HasPrecision(7, 2);
        builder.Property(pv => pv.Price).HasPrecision(10, 2);

        builder.Property(pv => pv.ImageUrl).HasMaxLength(500);

        // Версії продукту — тільки для читання після створення.
        // Якщо продукт видаляється — версії залишаються (Restrict),
        // бо на них можуть посилатись RecipeVersionIngredient.
        builder.HasOne(pv => pv.Product)
            .WithMany(p => p.Versions)
            .HasForeignKey(pv => pv.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pv => pv.ProductId);
        builder.HasIndex(pv => new { pv.ProductId, pv.VersionNumber }).IsUnique();
    }
}