using Application.Base.Interfaces;
using Domain.Entities;

namespace Application.Recipes.Interfaces;

public interface IRecipeRepository : IBaseRepository<Recipe>
{
    Task SaveRecipeAsync(int recipeId, string userId);
    
    Task<Recipe> CreateRecipeAsync(Recipe recipe);

    Task SaveChangesAsync();

    Task<bool> HasDayPlanEntriesAsync(int id);
    Task<bool> HasDiaryEntriesAsync(int id);
    
};