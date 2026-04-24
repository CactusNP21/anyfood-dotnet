using Application.RecipeCategories.DTOs;
using Application.RecipeCategories.Interfaces;
using Domain.Entities;
using Mapster;

namespace Application.RecipeCategories.Services;

public class RecipeCategoryService(IRecipeCategoryRepository repository): IRecipeCategoryService
{
    public async Task<IReadOnlyList<RecipeCategoryDto>> GetAllAsync()
    {
        var categories = await repository.GetAllAsync();
        return categories.Select(rc => rc.Adapt<RecipeCategoryDto>()).ToList();
    }

    public async Task<RecipeCategoryDto> GetByIdAsync(int id)
    {
        var category = await repository.GetByIdAsync(id)
                       ?? throw new KeyNotFoundException("Категорію не знайдено.");
        return category.Adapt<RecipeCategoryDto>();
    }

    public async Task<RecipeCategoryDto> CreateAsync(RecipeCategoryCreateRequest request)
    {
        var created = await repository.CreateAsync(request.Adapt<RecipeCategory>());
        return created.Adapt<RecipeCategoryDto>();
    }

    public async Task<RecipeCategoryDto> UpdateAsync(int id, RecipeCategoryUpdateRequest request)
    {
        var category = await repository.GetByIdAsync(id)
                       ?? throw new KeyNotFoundException("Категорію не знайдено.");

        await repository.UpdateAsync(category);

        return category.Adapt<RecipeCategoryDto>();
        
    }

    public async Task DeleteAsync(int id)
    {
        var category = await repository.GetByIdAsync(id)
                       ?? throw new KeyNotFoundException("Категорію не знайдено.");
        
        await repository.DeleteAsync(category);
    }
}