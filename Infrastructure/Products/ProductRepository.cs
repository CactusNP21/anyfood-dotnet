using Application.Products.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Products;

public class ProductRepository(AppDbContext context): IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetAllAsync()
   => await context.Products.ToListAsync();

    public async Task<Product?> GetByIdAsync(int id)
    => await context.Products.
        Include(p => p.Category).
    Include(p => p.PriceHistory).
        FirstOrDefaultAsync(p => p.Id == id);

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
        
        var productVersion = product.Adapt<ProductVersion>();
        productVersion.Product = product;
        context.ProductVersions.Add(productVersion);
        
        await context.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        var latestVersion = await GetLatestVersionNumberAsync(product.Id);
        var productVersion = product.Adapt<ProductVersion>();
        productVersion.VersionNumber = latestVersion + 1;
        context.ProductVersions.Add(productVersion);

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

    // ── Версіонування ────────────────────────────────────────────────────────

    public async Task<int> GetLatestVersionNumberAsync(int productId)
        => await context.ProductVersions
            .Where(pv => pv.ProductId == productId)
            .OrderByDescending(pv => pv.VersionNumber)
            .Select(pv => pv.VersionNumber)
            .FirstOrDefaultAsync(); // повертає 0 якщо версій немає

    public async Task<ProductVersion> CreateVersionAsync(ProductVersion version)
    {
        context.ProductVersions.Add(version);
        await context.SaveChangesAsync();
        return version;
    }

    public async Task<ProductVersion?> GetProductVersionByIdAsync(int productVersionId)
        => await context.ProductVersions
            .FirstOrDefaultAsync(pv => pv.Id == productVersionId);
    
    public async Task<ProductVersion?> GetLatestVersionAsync(int productId)
        => await context.ProductVersions
            .Where(pv => pv.ProductId == productId)
            .OrderByDescending(pv => pv.VersionNumber)
            .FirstOrDefaultAsync();
}