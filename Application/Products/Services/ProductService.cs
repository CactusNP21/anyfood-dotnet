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
        // 1. Завантажуємо поточний стан продукту
        var product = await productRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Продукт не знайдено.");

        // 2. Зберігаємо snapshot ПОТОЧНОГО стану перед тим як змінити
        var latestVersion = await productRepository.GetLatestVersionNumberAsync(id);

        await productRepository.CreateVersionAsync(new ProductVersion
        {
            ProductId = product.Id,
            VersionNumber = latestVersion + 1,
            // Копіюємо всі поточні поля продукту
            Name = product.Name,
            Calories = product.Calories,
            Protein = product.Protein,
            Fat = product.Fat,
            Carbs = product.Carbs,
            Price = product.Price,
            GlycemicIndex = product.GlycemicIndex,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
        });

        // 3. Тепер застосовуємо нові дані
        var updated = await productRepository.UpdateAsync(request.Adapt<Product>());
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