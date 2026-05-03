using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class DayPlanConfiguration : IEntityTypeConfiguration<DayPlan>
{
    public void Configure(EntityTypeBuilder<DayPlan> builder)
    {
        builder.HasKey(d => d.Id);

        builder.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => new { d.UserId }).IsUnique();
    }
}

public class DayPlanEntryConfiguration : IEntityTypeConfiguration<DayPlanEntry>
{
    public void Configure(EntityTypeBuilder<DayPlanEntry> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.DayPlan)
            .WithMany(d => d.Entries)
            .HasForeignKey(e => e.DayPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Recipe)
            .WithMany()
            .HasForeignKey(e => e.RecipeId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Гарантуємо що заповнений рівно один FK
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_DayPlanEntry_RecipeOrProduct",
            "(\"RecipeId\" IS NOT NULL) != (\"ProductId\" IS NOT NULL)"
        ));
    }
}