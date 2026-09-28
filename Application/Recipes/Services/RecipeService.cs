using System.Net.Mime;
using Application.Auth.DTOs;
using Application.Images.Interfaces;
using Application.Products.DTOs;
using Application.Products.Interfaces;
using Application.RecipeCategories.DTOs;
using Application.RecipeCategories.Interfaces;
using Application.Recipes.DTOs;
using Application.Recipes.Interfaces;
using Application.Recipes.Models;
using Domain.Entities;
using Mapster;
using SixLabors.ImageSharp;

namespace Application.Recipes.Services;

public class RecipeService(
    IRecipeRepository repository,
    IImageStorageService imageStorageService,
    IProductRepository productRepository,
    IRecipeCategoryRepository recipeCategoryRepository) : IRecipeService
{
    private static readonly int[] MainImageWidths = [500, 1200];
    private static readonly int[] StepImageWidths = [500];

    public async Task<RecipeDto> CreateAsync(CreateRecipeRequest request, bool isAdmin)
    {
        var productIds = request.RecipeProducts.Select(i => i.ProductId).ToList();
        var products = await productRepository.GetByBatchIdAsync(productIds);

        ValidateRecipeCanBeCreated(request, products);
        ValidateImages(request);

        var mainImageUrl = await ProcessMainImageAsync(request);
        var steps = await ProcessStepsAsync(request); // see note below
        
        var categoriesIds = request.RecipeCategories.Select(i => i.Id).ToList();
        var categories = await recipeCategoryRepository.GetByBatchIdAsync(categoriesIds);
        
        var nutrition = CalculateNutritionPer100G(request, products);
        var recipe = CreateRecipeFromRequest(request, nutrition, categories, isAdmin, mainImageUrl, steps);

        var created = await repository.CreateRecipeAsync(recipe);

        return created.Adapt<RecipeDto>();
    }

    private Recipe CreateRecipeFromRequest(
        CreateRecipeRequest request,
        NutritionPer100G nutrition,
        IReadOnlyList<RecipeCategory> categories,
        bool isSystem,
        string imageUrl,
        List<RecipeStep> steps)
    {
        return new Recipe
        {
            Id = 0,
            Name = request.Name,
            Price = nutrition.Price,
            ImageUrl = imageUrl,
            RecipeProducts = request.RecipeProducts.Select(rp => new RecipeProduct
            {
                ProductId = rp.ProductId,
                Weight = rp.Weight,
            }).ToList(),
            RecipeCategories = categories.ToList(),
            Portions = request.Portions,
            Description = request.Description,
            Duration = request.Duration,
            Calories = nutrition.Calories,
            Protein = nutrition.Protein,
            Fat = nutrition.Fat,
            Carbs = nutrition.Carbs,
            UserId = request.UserId,
            IsSystem = isSystem,
            Steps = steps,
        };
    }

    private async Task<string> ProcessMainImageAsync(CreateRecipeRequest request)
    {
        await using var ms = new MemoryStream(request.Image);
        var result = await imageStorageService.SaveAsync(ms, MainImageWidths);
        return result.VariantUrls[MainImageWidths.Max()]; // 1200px як основний URL
    }

    private async Task<List<RecipeStep>> ProcessStepsAsync(CreateRecipeRequest request)
    {
        var steps = new List<RecipeStep>();

        foreach (var step in request.Steps)
        {
            string? imageUrl = null;

            if (step.Image is not null)
            {
                await using var ms = new MemoryStream(step.Image);
                var result = await imageStorageService.SaveAsync(ms, StepImageWidths);
                imageUrl = result.VariantUrls[StepImageWidths.Max()];
            }

            steps.Add(new RecipeStep
            {
                Order = step.Order,
                Description = step.Description,
                Timer = step.Timer,
                ImageUrl = imageUrl,
            });
        }

        return steps;
    }

    private static void ValidateImages(CreateRecipeRequest request)
    {
        if (request.Image is null)
            throw new InvalidOperationException("Потрібно вказати зображення рецепту — файл або URL.");

        EnsureValidImage(request.Image, "Головне зображення рецепту пошкоджене або має непідтримуваний формат.");

        foreach (var step in request.Steps.Where(s => s.Image is not null))
            EnsureValidImage(step.Image!, $"Зображення кроку {step.Order} пошкоджене або має непідтримуваний формат.");
    }

    private static void EnsureValidImage(byte[] bytes, string errorMessage)
    {
        try
        {
            Image.Identify(bytes);
        }
        catch (Exception ex) when (
            ex is NotSupportedException
                or InvalidImageContentException
                or UnknownImageFormatException)
        {
            throw new InvalidOperationException(errorMessage, ex);
        }
    }


    private static void ValidateRecipeCanBeCreated(CreateRecipeRequest request, IReadOnlyList<Product> products)
    {
        if (request.RecipeProducts.Count == 0)
            throw new InvalidOperationException("Рецепт повинен містити хоча б один інгредієнт.");
        
        if (request.RecipeProducts.Any(rp => rp.Weight <= 0))
            throw new InvalidOperationException("Вага кожного інгредієнта повинна бути більше 0.");

        var missingIds = request.RecipeProducts
            .Select(rp => rp.ProductId)
            .Except(products.Select(p => p.Id))
            .ToList();

        if (missingIds.Count > 0)
            throw new KeyNotFoundException($"Продукти не знайдено: {string.Join(", ", missingIds)}.");
    }

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

    private static RecipeDto ConvertRecipeToDto(Recipe recipe)
    {
        return new RecipeDto
        {
            Id = recipe.Id,
            Name = recipe.Name,
            Price = recipe.Price,
            ImageUrl = recipe.ImageUrl,
            Products = recipe.RecipeProducts.Select(rp =>
            {
                var dto = rp.Product.Adapt<ProductDto>();
                dto.Weight = rp.Weight;
                return dto;
            }).ToList(),
            RecipeCategories = recipe.RecipeCategories
                .Select(rc => rc.Adapt<RecipeCategoryDto>())
                .ToList(),
            Description = recipe.Description,
            Duration = recipe.Duration,
            Calories = recipe.Calories,
            Protein = recipe.Protein,
            Fat = recipe.Fat,
            Carbs = recipe.Carbs,
            UserId = recipe.UserId,
            User = recipe.User is null ? null : new UserDto
            {
                Id = recipe.User.Id,
                Name = recipe.User.Name,
                Username = recipe.User.UserName!,
                Email = recipe.User.Email!,
                AvatarUrl = recipe.User.AvatarUrl,
            },
            Steps = recipe.Steps
                .OrderBy(s => s.Order)
                .Select(s => new RecipeStepDto
                {
                    Order = s.Order,
                    Description = s.Description,
                    Timer = s.Timer,
                    ImageUrl = s.ImageUrl,
                })
                .ToList(),
        };
    }

    private static NutritionPer100G CalculateNutritionPer100G(
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

    public async Task<RecipeDto> UpdateAsync(int id, UpdateRecipeRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task SaveRecipe(int recipeId, string userId)
    {
        var recipe = await repository.GetByIdAsync(recipeId) ?? throw new KeyNotFoundException();
        await repository.SaveRecipeAsync(recipe.Id, userId);
    }


    public async Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }
}