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

        builder.HasIndex(rvi => rvi.RecipeVersionId);
    }
}