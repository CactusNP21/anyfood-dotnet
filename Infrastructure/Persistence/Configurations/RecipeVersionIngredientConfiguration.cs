using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class RecipeVersionIngredientConfiguration : IEntityTypeConfiguration<RecipeVersionIngredient>
{
    public void Configure(EntityTypeBuilder<RecipeVersionIngredient> builder)
    {
        builder.HasKey(rvi => rvi.Id);

        builder.Property(rvi => rvi.Weight).IsRequired();

        // Якщо версія рецепту видаляється — інгредієнти теж
        builder.HasOne(rvi => rvi.RecipeVersion)
            .WithMany(rv => rv.Ingredients)
            .HasForeignKey(rvi => rvi.RecipeVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Якщо версія продукту видаляється — забороняємо (Restrict)
        // бо це зламає історичні дані рецепту
        builder.HasOne(rvi => rvi.ProductVersion)
            .WithMany()
            .HasForeignKey(rvi => rvi.ProductVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(rvi => rvi.RecipeVersionId);
        builder.HasIndex(rvi => rvi.ProductVersionId);
    }
}