using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class FridgeConfiguration : IEntityTypeConfiguration<Domain.Entities.Fridge>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Fridge> builder)
    {
        builder.HasKey(f => f.Id);

        builder.HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Один холодильник на юзера
        builder.HasIndex(f => f.UserId).IsUnique();
    }
}

public class FridgeItemConfiguration : IEntityTypeConfiguration<FridgeItem>
{
    public void Configure(EntityTypeBuilder<FridgeItem> builder)
    {
        builder.HasKey(fi => fi.Id);

        builder.Property(fi => fi.Weight).IsRequired();

        builder.HasOne(fi => fi.Fridge)
            .WithMany(f => f.Items)
            .HasForeignKey(fi => fi.FridgeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fi => fi.Product)
            .WithMany()
            .HasForeignKey(fi => fi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Один рядок на продукт у межах одного холодильника (агрегована модель)
        builder.HasIndex(fi => new { fi.FridgeId, fi.ProductId }).IsUnique();
    }
}