using Application.Products.DTOs;
using Domain.Entities;

namespace Application.Products.Interfaces;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync();
    Task<IReadOnlyList<Product>> FilterAsync(ProductFilterRequest filter);
    Task<Product?> GetByIdAsync(int id);
    Task<IReadOnlyList<Product>> GetByBatchIdAsync(List<int> categoryId);
    Task<Product?> GetByNameAsync(string name);
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task DeleteAsync(Product product);
    
    Task<bool> HasRecipesAsync(int id);
}