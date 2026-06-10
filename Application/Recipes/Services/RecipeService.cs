using Application.Products.DTOs;
using Application.Products.Interfaces;
using Application.Recipes.DTOs;
using Application.Recipes.Interfaces;
using Application.Recipes.Models;
using Domain.Entities;
using Mapster;

namespace Application.Recipes.Services;

public class RecipeService(IRecipeRepository repository, IProductRepository productRepository) : IRecipeService
{
    public async Task<IReadOnlyList<RecipeDto>> GetAllAsync()
    {
        var recipes = await repository.GetAllAsync();
        return recipes.Select(r => r.Adapt<RecipeDto>()).ToList();
    }

    public async Task<RecipeDto> GetByIdAsync(int id)
    {
        var recipe = await repository.GetByIdAsync(id) ?? throw new KeyNotFoundException("Рецепт не знайдено");

        var dto = recipe.Adapt<RecipeDto>();
        dto.Products = recipe.RecipeProducts
            .Select(rp =>
            {
                var productDto = rp.Product.Adapt<ProductDto>();
                productDto.Weight = rp.Weight;
                return productDto;
            })
            .ToList();

        return dto;
    }

    private NutritionPer100G CalculateNutritionPer100G(
        CreateRecipeRequest request, IReadOnlyList<Product> products)
    {
        float calories = 0, protein = 0, fat = 0, carbs = 0, totalPrice = 0;

        foreach (var ingredient in request.RecipeProducts)
        {
            var product = products.First(p => p.Id == ingredient.ProductId);
            var ratio = ingredient.Weight / 100f;

            calories += (float)product.Calories * ratio;
            protein += (float)product.Protein * ratio;
            fat += (float)product.Fat * ratio;
            carbs += (float)product.Carbs * ratio;
            totalPrice += (float)product.Price * ratio;
        }

        var per100 = 100f / request.RecipeProducts.Sum(r => r.Weight);

        return new NutritionPer100G(
            calories * per100, protein * per100,
            fat * per100, carbs * per100, totalPrice * per100);
    }

    public async Task<RecipeDto> CreateAsync(CreateRecipeRequest request, bool isAdmin)
    {
        var productIds = request.RecipeProducts.Select(i => i.ProductId).ToList();
        var products = await productRepository.GetByBatchIdAsync(productIds);

        var nutrition = CalculateNutritionPer100G(request, products);

        var recipe = CreateRecipeFromRequest(request, nutrition, isAdmin);

        var recipeVersion = new RecipeVersion
        {
            Id = 0,
            RecipeId = 0,
            Recipe = recipe,
            VersionNumber = 1,
            Name = recipe.Name,
            Description = recipe.Description,
            ImageUrl = recipe.ImageUrl,
            Portions = recipe.Portions,
            Duration = recipe.Duration,
            Calories = recipe.Calories,
            Protein = recipe.Protein,
            Fat = recipe.Fat,
            Carbs = recipe.Carbs,
            Price = recipe.Price,
            Ingredients = request.RecipeProducts.Select(rp => new RecipeVersionIngredient
                {
                    ProductId = rp.ProductId,
                    Weight = rp.Weight,
                })
                .ToList(),
            CreatedAt = default,
            CreatedByUserId = null,
            CreatedByUser = null,
        };

        var created = await repository.CreateRecipeVersionAsync(recipe, recipeVersion);

        created.LatestVersionId = recipeVersion.Id;

        return created.Adapt<RecipeDto>();
    }

    private Recipe CreateRecipeFromRequest(CreateRecipeRequest request, NutritionPer100G nutrition, bool isAdmin)
    {
        return new Recipe
        {
            Id = 0,
            Name = request.Name,
            Price = nutrition.Price,
            ImageUrl = request.ImageUrl,
            RecipeProducts = request.RecipeProducts.Select(rp => new RecipeProduct
                    {
                        ProductId = rp.ProductId,
                        Weight = rp.Weight,
                    }
                )
                .ToList(),
            RecipeCategories = request.RecipeCategories.Select(rc => new RecipeCategory
                {
                    Id = rc.Id
                })
                .ToList(),
            Portions = request.Portions,
            Description = request.Description,
            Duration = request.Duration,
            Calories = nutrition.Calories,
            Protein = nutrition.Protein,
            Fat = nutrition.Fat,
            Carbs = nutrition.Carbs,
            UserId = request.UserId,
        };
    }

    public async Task<RecipeDto> UpdateAsync(int id, UpdateRecipeRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task SaveRecipe(int recipeId, string userId)
    {
        var version = await repository.GetLatestVersionAsync(recipeId) ?? throw new KeyNotFoundException();
        await repository.SaveRecipeAsync(version.Id, userId);
    }


    public async Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }
}