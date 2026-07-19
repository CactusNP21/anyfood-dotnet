using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        
        builder.HasMany(r => r.RecipeCategories)
            .WithMany()
            .UsingEntity("RecipeCategoryRecipes"); // actual join table

    }
    
    
}