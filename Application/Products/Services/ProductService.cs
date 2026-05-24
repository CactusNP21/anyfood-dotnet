using Application.Products.DTOs;
using Application.Products.Interfaces;
using Domain.Entities;
using Mapster;

namespace Application.Products.Services;

public class ProductService(IProductRepository productRepository) : IProductService
{
    public async Task<IReadOnlyList<ProductSummaryDto>> GetAllAsync()
    {
        var products = await productRepository.GetAllAsync();
        return products.Select(product => product.Adapt<ProductSummaryDto>()).ToList();
    }
    
    public async Task<IReadOnlyList<ProductSummaryDto>> FilterAsync(ProductFilterRequest filter)
    {
        var products = await productRepository.FilterAsync(filter);
        return products.Select(p => p.Adapt<ProductSummaryDto>()).ToList();
    }

    public async Task<ProductDto> GetByIdAsync(int id)
    {
        var product = await productRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Продукт не знайдено.");

        return product.Adapt<ProductDto>();
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request)
    {
        
        var created = await productRepository.CreateAsync(request.Adapt<Product>());
        return created.Adapt<ProductDto>();
    }

    public async Task<ProductDto> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = await productRepository.GetByIdAsync(id)
                      ?? throw new KeyNotFoundException("Продукт не знайдено.");

        // Мапимо request на існуючий відслідковуваний об'єкт
        request.Adapt(product);

        var updated = await productRepository.UpdateAsync(product);
        return updated.Adapt<ProductDto>();
    }

    public async Task DeleteAsync(int id)
    {
        var product = await productRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Продукт не знайдено.");

        var hasRecipes = await productRepository.HasRecipesAsync(id);
        if (hasRecipes)
            throw new InvalidOperationException(
                "Неможливо видалити продукт, який використовується в рецептах.");

        await productRepository.DeleteAsync(product);
    }
}