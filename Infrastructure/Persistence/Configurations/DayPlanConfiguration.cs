using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class DayPlanConfiguration : IEntityTypeConfiguration<DayPlan>
{
    public void Configure(EntityTypeBuilder<DayPlan> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Індекс для швидкого пошуку планів юзера
        builder.HasIndex(d => d.UserId);
    }
}

public class DayPlanEntryConfiguration : IEntityTypeConfiguration<DayPlanEntry>
{
    public void Configure(EntityTypeBuilder<DayPlanEntry> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Weight).IsRequired();

        builder.HasOne(e => e.DayPlan)
            .WithMany(d => d.Entries)
            .HasForeignKey(e => e.DayPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        // Версія рецепту — Restrict, щоб не можна було видалити версію,
        // яка використовується в плані
        builder.HasOne(e => e.RecipeVersion)
            .WithMany()
            .HasForeignKey(e => e.RecipeVersionId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
        

        // DB-рівень: рівно один із двох FK заповнений
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_DayPlanEntry_RecipeVersionOrProductVersion",
            "(\"RecipeVersionId\" IS NOT NULL) != (\"ProductId\" IS NOT NULL)"
        ));

        builder.HasIndex(e => e.DayPlanId);
    }
}