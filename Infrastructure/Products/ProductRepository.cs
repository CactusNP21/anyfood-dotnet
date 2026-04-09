using Application.Products.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Products;

public class ProductRepository(AppDbContext context): IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetAllAsync()
   => await context.Products.ToListAsync();

    public async Task<Product?> GetByIdAsync(int id)
    => await context.Products.FindAsync(id);

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
        context.Products.Update(product);
        await context.SaveChangesAsync();
        return product;
    }

    public Task DeleteAsync(Product product)
    {
        context.Products.Remove(product);
        return context.SaveChangesAsync();
    }
}