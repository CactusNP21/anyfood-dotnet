using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.HasOne(r => r.LatestVersion)
            .WithOne()
            .HasForeignKey<Recipe>(r => r.LatestVersionId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}