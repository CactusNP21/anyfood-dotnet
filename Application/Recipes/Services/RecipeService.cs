using Application.Products.DTOs;
using Application.Products.Interfaces;
using Application.Recipes.DTOs;
using Application.Recipes.Interfaces;
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
    

    public async Task<RecipeDto> CreateAsync(CreateRecipeRequest request)
    {
        var productIds = request.RecipeProducts.Select(i => i.ProductId).ToList();
        var products = await productRepository.GetByBatchIdAsync(productIds);

        var totalWeight = request.RecipeProducts.Sum(r => r.Weight);
          
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

        // Перераховуємо на 100г рецепту
        var per100 = 100f / totalWeight;
        var recipe = new Recipe
        {
            Id = 0,
            Name = request.Name,
            ImageUrl = request.ImageUrl,
            Portions = request.Portions,
            Description = request.Description,
            Duration = request.Duration,
            UserId = request.UserId,
            RecipeProducts = request.RecipeProducts.Select(rp => new RecipeProduct
            {
                ProductId = rp.ProductId,
                Weight = rp.Weight
            }).ToList(),
            RecipeCategories = request.RecipeCategories.Adapt<ICollection<RecipeCategory>>(),
            Price    = totalPrice * per100,
            Calories = calories   * per100,
            Protein  = protein    * per100,
            Fat      = fat        * per100,
            Carbs    = carbs      * per100,
        };

        var created = await repository.CreateAsync(recipe);
        
        // Для кожного інгредієнта беремо версію 1 продукту
        var versionIngredients = new List<RecipeVersionIngredient>();

        foreach (var rp in created.RecipeProducts)
        {
            var productVersion = await productRepository.GetLatestVersionAsync(rp.ProductId)
                                 ?? throw new InvalidOperationException(
                                     $"Продукт з id={rp.ProductId} не має жодної версії.");

            versionIngredients.Add(new RecipeVersionIngredient
            {
                ProductVersionId = productVersion.Id,
                Weight = rp.Weight,
            });
        }
        return created.Adapt<RecipeDto>();
    }

    public async Task<RecipeDto> UpdateAsync(int id, UpdateRecipeRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task SaveRecipe(int recipeId, string userId)
    {
        _ = await repository.GetByIdAsync(recipeId) ?? throw new KeyNotFoundException();
        await repository.SaveRecipeAsync(recipeId, userId);
    }


    public async Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }
}