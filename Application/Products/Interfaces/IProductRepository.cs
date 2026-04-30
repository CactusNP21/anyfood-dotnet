using Domain.Entities;

namespace Application.Products.Interfaces;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<IReadOnlyList<Product>> GetByBatchIdAsync(List<int> categoryId);
    Task<Product?> GetByNameAsync(string name);
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task DeleteAsync(Product product);
    
    Task<bool> HasRecipesAsync(int id);
    
    // ── Версіонування ────────────────────────────────────────────────────────
    // Повертає останній номер версії для продукту (0 якщо версій ще немає)
    Task<int> GetLatestVersionNumberAsync(int productId);
    // Зберігає snapshot продукту перед оновленням
    Task<ProductVersion> CreateVersionAsync(ProductVersion version);
    // Повертає конкретну версію продукту по її Id
    Task<ProductVersion?> GetProductVersionByIdAsync(int productVersionId);
    Task<ProductVersion?> GetLatestVersionAsync(int productId);
}