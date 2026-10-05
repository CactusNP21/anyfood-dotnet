using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class FoodDiaryDayConfiguration : IEntityTypeConfiguration<FoodDiaryDay>
{
    public void Configure(EntityTypeBuilder<FoodDiaryDay> builder)
    {
        builder.HasKey(d => d.Id);

        builder.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Один день щоденника на юзера на дату
        builder.HasIndex(d => new { d.UserId, d.Date }).IsUnique();
    }
}

public class FoodDiaryEntryConfiguration : IEntityTypeConfiguration<FoodDiaryEntry>
{
    public void Configure(EntityTypeBuilder<FoodDiaryEntry> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Weight).IsRequired();

        builder.HasOne(e => e.FoodDiaryDay)
            .WithMany(d => d.Entries)
            .HasForeignKey(e => e.FoodDiaryDayId)
            .OnDelete(DeleteBehavior.Cascade);

        // Не даємо видалити продукт/рецепт, який є в історії щоденника
        builder.HasOne(e => e.Product)
            .WithMany()
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Recipe)
            .WithMany()
            .HasForeignKey(e => e.RecipeId)
            .OnDelete(DeleteBehavior.Restrict);

        // DB-рівень: рівно один із двох FK заповнений
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_FoodDiaryEntry_RecipeOrProduct",
            "(\"RecipeId\" IS NOT NULL) != (\"ProductId\" IS NOT NULL)"
        ));

        builder.HasIndex(e => e.FoodDiaryDayId);
    }
}

public class CalorieGoalConfiguration : IEntityTypeConfiguration<CalorieGoal>
{
    public void Configure(EntityTypeBuilder<CalorieGoal> builder)
    {
        builder.HasKey(g => g.Id);

        builder.HasOne(g => g.User)
            .WithMany()
            .HasForeignKey(g => g.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Одна зміна цілі на дату; індекс також для пошуку "останньої цілі <= дата"
        builder.HasIndex(g => new { g.UserId, g.EffectiveFrom }).IsUnique();
    }
}
