using Application.Categories.DTOs;
using Application.Categories.Interfaces;
using Domain.Entities;

namespace Application.Categories.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        this.categoryRepository = categoryRepository;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync()
    {
        var categories = await categoryRepository.GetAllAsync();
        return categories.Select(ToDto).ToList();
    }

    public async Task<CategoryDto> GetByIdAsync(int id)
    {
        var category = await categoryRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Категорію не знайдено.");

        return ToDto(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request)
    {
        var existing = await categoryRepository.GetByNameAsync(request.Name);
        if (existing is not null)
            throw new InvalidOperationException($"Категорія з назвою '{request.Name}' вже існує.");

        var category = new Category { Name = request.Name };
        var created = await categoryRepository.CreateAsync(category);

        return ToDto(created);
    }

    public async Task<CategoryDto> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var category = await categoryRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Категорію не знайдено.");

        var duplicate = await categoryRepository.GetByNameAsync(request.Name);
        if (duplicate is not null && duplicate.Id != id)
            throw new InvalidOperationException($"Категорія з назвою '{request.Name}' вже існує.");

        category.Name = request.Name;
        await categoryRepository.UpdateAsync(category);

        return ToDto(category);
    }

    public async Task DeleteAsync(int id)
    {
        var category = await categoryRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Категорію не знайдено.");

        var hasProducts = await categoryRepository.HasProductsAsync(id);
        if (hasProducts)
            throw new InvalidOperationException(
                "Неможливо видалити категорію, до якої прив'язані продукти. Спочатку перенесіть або видаліть продукти.");

        await categoryRepository.DeleteAsync(category);
    }

    private static CategoryDto ToDto(Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        ProductCount = category.Products.Count,
    };
}