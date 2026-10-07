using Application.Base.Interfaces;
using Application.Recipes.DTOs;

namespace Application.Recipes.Interfaces;

public interface IRecipeService
{
    public Task SaveRecipe(int recipeId, string userId);

    public Task<RecipeDto> CreateAsync(CreateRecipeRequest request, bool isAdmin);
    
    Task<IReadOnlyList<RecipeDto>> GetAllAsync();
    Task<RecipeDto> GetByIdAsync(int id);
    Task<RecipeDto> UpdateAsync(int id, UpdateRecipeRequest request, bool isAdmin);
    Task DeleteAsync(int id, string? userId, bool isAdmin);
};