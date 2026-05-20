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