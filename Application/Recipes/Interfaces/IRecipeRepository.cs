using Application.Base.Interfaces;
using Domain.Entities;

namespace Application.Recipes.Interfaces;

public interface IRecipeRepository : IBaseRepository<Recipe>
{
    Task SaveRecipeAsync(int recipeId, string userId);
    // ── Версіонування ────────────────────────────────────────────────────────
    // Повертає останній номер версії для рецепту (0 якщо версій ще немає)
    Task<int> GetLatestVersionNumberAsync(int recipeId);
    // Зберігає snapshot рецепту разом з інгредієнтами
    Task<RecipeVersion> CreateVersionAsync(RecipeVersion version);
    // Повертає всі версії рецепту (без інгредієнтів — для списку)
    Task<IReadOnlyList<RecipeVersion>> GetVersionsAsync(int recipeId);
    // Повертає конкретну версію з інгредієнтами та даними продуктів
    Task<RecipeVersion?> GetVersionByIdAsync(int recipeId, int versionId);

};