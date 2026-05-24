using Application.Products.DTOs;
using Application.Products.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Products;

public class ProductRepository(AppDbContext context) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetAllAsync()
        => await context.Products.ToListAsync();
    public async Task<IReadOnlyList<Product>> FilterAsync(ProductFilterRequest filter)
    {
        var query = context.Products.AsQueryable();

        if (filter.CategoryIds is { Count: > 0 })
            query = query.Where(p => filter.CategoryIds.Contains(p.CategoryId));

        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.Price >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(p => p.Name.ToLower().Contains(filter.Name.ToLower()));

        if (filter.IsSystem.HasValue)
            query = query.Where(p => p.IsSystem == filter.IsSystem.Value);

        return await query.ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
        => await context.Products.Include(p => p.Category).Include(p => p.PriceHistory)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IReadOnlyList<Product>> GetByBatchIdAsync(List<int> categoryIds)
        => await context.Products
            .AsNoTracking()
            .Where(p => ((IEnumerable<int>)categoryIds).Contains(p.Id))
            .ToListAsync();

    public async Task<Product?> GetByNameAsync(string name)
        => await context.Products.FirstOrDefaultAsync(p => p.Name == name);

    public async Task<Product> CreateAsync(Product product)
    {
        context.Products.Add(product);

        await context.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {

await context.SaveChangesAsync(); // збереже і зміни продукту і нову версію
        return product;

    }

    public Task DeleteAsync(Product product)
    {
        context.Products.Remove(product);
        return context.SaveChangesAsync();
    }

    public async Task<bool> HasRecipesAsync(int id)
    
        => await context.RecipeProducts.AnyAsync(rp => rp.ProductId == id);
    
    
}