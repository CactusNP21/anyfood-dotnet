using Application.Categories.Interfaces;
using Application.Products.DTOs;
using Application.Products.Interfaces;
using Domain.Entities;
using Mapster;

namespace Application.Products.Services;

public class ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository) : IProductService
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

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, string userId, bool isSystem)
    {
        var categories = await categoryRepository.GetByIdsAsync(request.CategoryIds);

        var product = new Product
        {
            Id = 0,
            Name = request.Name,
            Calories = CalculateCalories(request.Carbs, request.Fat, request.Protein),
            Protein = request.Protein,
            Fat = request.Fat,
            Carbs = request.Carbs,
            GlycemicIndex = null,
            ImageUrl = request.ImageUrl,
            Price = request.Price,
            IsSystem = isSystem,
            Categories = categories.ToList(),
            UserId = userId,
            User = null
        };
        
        var created = await productRepository.CreateAsync(product);
        return created.Adapt<ProductDto>();

    }

    private decimal CalculateCalories(decimal carbs, decimal fats, decimal proteins)
    {
        return carbs * 4 + fats * 9 + proteins * 4;
    }
    
    public async Task<ProductDto> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = await productRepository.GetByIdAsync(id)
                      ?? throw new KeyNotFoundException("Продукт не знайдено.");

        product.Name = request.Name;
        product.Carbs = request.Carbs;
        product.Fat = request.Fat;
        product.Protein = request.Protein;
        product.Calories = CalculateCalories(request.Carbs, request.Fat, request.Protein);
        product.Price = request.Price;
        product.ImageUrl = request.ImageUrl;
        
        var categories = await categoryRepository.GetByIdsAsync(request.Categories);
        product.Categories = categories.ToList();

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