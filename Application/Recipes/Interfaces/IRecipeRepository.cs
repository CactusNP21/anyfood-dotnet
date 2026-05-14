using Application.Base.Interfaces;
using Domain.Entities;

namespace Application.Recipes.Interfaces;

public interface IRecipeRepository : IBaseRepository<Recipe>
{
    Task SaveRecipeAsync(int recipeId, string userId);
    
    Task<Recipe> CreateRecipeVersionAsync(Recipe recipe, RecipeVersion recipeVersion);

    // ── Версіонування ────────────────────────────────────────────────────────
    // Повертає останній номер версії для рецепта (0, якщо версій ще немає)
    Task<int> GetLatestVersionNumberAsync(int recipeId);
    // Повертає всі версії рецепта (без інгредієнтів — для списку)
    Task<IReadOnlyList<RecipeVersion>> GetVersionsAsync(int recipeId);
    // Повертає конкретну версію з інгредієнтами та даними продуктів
    Task<RecipeVersion?> GetVersionByIdAsync(int versionId);
    
    // Application/Recipes/Interfaces/IRecipeRepository.cs
    Task<RecipeVersion?> GetLatestVersionAsync(int recipeId);

};