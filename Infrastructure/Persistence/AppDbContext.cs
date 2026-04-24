using Domain.Common;
using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User>(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Domain.Entities.RecipeCategory> RecipeCategories => Set<Domain.Entities.RecipeCategory>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ProductPriceHistory> ProductPriceHistories => Set<ProductPriceHistory>();
    public DbSet<RecipeProduct> RecipeProducts => Set<RecipeProduct>();
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RecipeProduct>()
            .HasKey(rp => new { rp.RecipeId, rp.ProductId });

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}