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

            calories   += (float)product.Calories * ratio;
            protein    += (float)product.Protein  * ratio;
            fat        += (float)product.Fat      * ratio;
            carbs      += (float)product.Carbs    * ratio;
            totalPrice += (float)product.Price    * ratio;
        }

        var per100 = 100f / request.RecipeProducts.Sum(r => r.Weight);

        return new NutritionPer100G(
            calories * per100, protein * per100,
            fat * per100, carbs * per100, totalPrice * per100);
    }

    public async Task<RecipeDto> CreateAsync(CreateRecipeRequest request)
    {
        var productIds = request.RecipeProducts.Select(i => i.ProductId).ToList();
        var products = await productRepository.GetByBatchIdAsync(productIds);

        // Знаходимо актуальні версії всіх продуктів одразу
        var productVersions = new Dictionary<int, ProductVersion>();
        foreach (var productId in productIds)
        {
            var version = await productRepository.GetLatestVersionAsync(productId)
                          ?? throw new InvalidOperationException(
                              $"Продукт з id={productId} не має жодної версії.");
            productVersions.Add(productId, version);
        }
        
        var nutrition = CalculateNutritionPer100G(request, products);

        var recipe = request.Adapt<Recipe>();
        nutrition.Adapt(recipe); // накладаємо БЖВ на існуючий об'єкт
        
        var recipeVersion = new RecipeVersion
        {
            Recipe = recipe,
            VersionNumber = 1,
            Ingredients = request.RecipeProducts.Select(rp => new RecipeVersionIngredient
            {
                ProductVersionId = productVersions[rp.ProductId].Id,
                Weight = rp.Weight,
            }).ToList(),
        };
        request.Adapt(recipeVersion); // копіює Name, Description, ImageUrl, Portions, Duration
        recipeVersion.Calories = nutrition.Calories;
        recipeVersion.Protein  = nutrition.Protein;
        recipeVersion.Fat      = nutrition.Fat;
        recipeVersion.Carbs    = nutrition.Carbs;
        recipeVersion.Price    = nutrition.Price;
        
        var created = await repository.CreateRecipeVersionAsync(recipe, recipeVersion);
        
        created.LatestVersionId = recipeVersion.Id;

        return created.Adapt<RecipeDto>();
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