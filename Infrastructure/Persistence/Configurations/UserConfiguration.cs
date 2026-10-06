using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Enum-и як рядки — порядок значень можна змінювати без міграції даних
        builder.Property(u => u.Sex).HasConversion<string>().HasMaxLength(16);
        builder.Property(u => u.ActivityLevel).HasConversion<string>().HasMaxLength(16);
        builder.Property(u => u.WeightGoal).HasConversion<string>().HasMaxLength(16);

        // До одного знаку після коми: 250.0 см, 300.0 кг
        builder.Property(u => u.HeightCm).HasPrecision(4, 1);
        builder.Property(u => u.WeightKg).HasPrecision(4, 1);
    }
}
