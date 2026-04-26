using Application.Base.Interfaces;
using Application.Recipes.DTOs;

namespace Application.Recipes.Interfaces;

public interface IRecipeService : IBaseService<RecipeDto, CreateRecipeRequest, UpdateRecipeRequest>
{
    public Task SaveRecipe(int recipeId, string userId);
};