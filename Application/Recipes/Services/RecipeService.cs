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
    IRecipeCategoryRepository recipeCategoryRepository,
    IImageCleanupService imageCleanupService) : IRecipeService
{
    private static readonly int[] MainImageWidths = [500, 1200];
    private static readonly int[] StepImageWidths = [500];

    public async Task<RecipeDto> CreateAsync(CreateRecipeRequest request, bool isAdmin)
    {
        var productIds = request.RecipeProducts.Select(i => i.ProductId).ToList();
        var products = await productRepository.GetByBatchIdAsync(productIds);

        ValidateIngredients(request.RecipeProducts, products);
        ValidateImages(request);

        var mainImageUrl = await ProcessMainImageAsync(request);
        var steps = await ProcessStepsAsync(request); // see note below
        
        var categoriesIds = request.RecipeCategories.Select(i => i.Id).ToList();
        var categories = await recipeCategoryRepository.GetByBatchIdAsync(categoriesIds);
        
        var nutrition = CalculateNutritionPer100G(request.RecipeProducts, products);
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
            Salt = nutrition.Salt,
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


    private static void ValidateIngredients(ICollection<RecipeIngredientDto> ingredients, IReadOnlyList<Product> products)
    {
        if (ingredients.Count == 0)
            throw new InvalidOperationException("Рецепт повинен містити хоча б один інгредієнт.");
        
        if (ingredients.Any(rp => rp.Weight <= 0))
            throw new InvalidOperationException("Вага кожного інгредієнта повинна бути більше 0.");

        if (ingredients.GroupBy(rp => rp.ProductId).Any(g => g.Count() > 1))
            throw new InvalidOperationException("Кожен продукт може бути в рецепті лише один раз.");

        var missingIds = ingredients
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
        dto.Steps = dto.Steps.OrderBy(s => s.Order).ToList();

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
            Salt = recipe.Salt,
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
        ICollection<RecipeIngredientDto> ingredients, IReadOnlyList<Product> products)
    {
        float calories = 0, protein = 0, fat = 0, carbs = 0, salt = 0, totalPrice = 0;

        foreach (var ingredient in ingredients)
        {
            var product = products.First(p => p.Id == ingredient.ProductId);
            var ratio = ingredient.Weight / 100f;

            calories += (float)product.Calories * ratio;
            protein += (float)product.Protein * ratio;
            fat += (float)product.Fat * ratio;
            carbs += (float)product.Carbs * ratio;
            salt += (float)product.Salt * ratio;
            totalPrice += (float)product.Price * ratio;
        }

        var per100 = 100f / ingredients.Sum(r => r.Weight);

        return new NutritionPer100G(
            calories * per100, protein * per100,
            fat * per100, carbs * per100, salt * per100, totalPrice * per100);
    }

    public async Task<RecipeDto> UpdateAsync(int id, UpdateRecipeRequest request, bool isAdmin)
    {
        var recipe = await repository.GetByIdAsync(id) ?? throw new KeyNotFoundException("Рецепт не знайдено");
        EnsureCanModify(recipe, request.UserId, isAdmin, "Ви можете редагувати лише власні рецепти.");

        var productIds = request.RecipeProducts.Select(i => i.ProductId).ToList();
        var products = await productRepository.GetByBatchIdAsync(productIds);

        ValidateIngredients(request.RecipeProducts, products);
        ValidateUpdateSteps(request, recipe);

        var previousImages = CollectImageUrls(recipe);

        // Нові файли зберігаємо лише після всіх перевірок
        if (request.Image is not null)
        {
            EnsureValidImage(request.Image, "Головне зображення рецепту пошкоджене або має непідтримуваний формат.");
            recipe.ImageUrl = await SaveImageAsync(request.Image, MainImageWidths);
        }

        await ApplyStepsAsync(recipe, request.Steps);
        ApplyIngredients(recipe, request.RecipeProducts);

        var categoryIds = request.RecipeCategories.Select(c => c.Id).ToList();
        var categories = await recipeCategoryRepository.GetByBatchIdAsync(categoryIds);
        ApplyCategories(recipe, categories);

        var nutrition = CalculateNutritionPer100G(request.RecipeProducts, products);
        recipe.Name = request.Name;
        recipe.Portions = request.Portions ?? recipe.Portions;
        recipe.Description = request.Description;
        recipe.Duration = request.Duration;
        recipe.Price = nutrition.Price;
        recipe.Calories = nutrition.Calories;
        recipe.Protein = nutrition.Protein;
        recipe.Fat = nutrition.Fat;
        recipe.Carbs = nutrition.Carbs;
        recipe.Salt = nutrition.Salt;

        await repository.SaveChangesAsync();
        await imageCleanupService.DeleteUnusedAsync(previousImages);

        return await GetByIdAsync(id);
    }

    private static void ValidateUpdateSteps(UpdateRecipeRequest request, Recipe recipe)
    {
        if (request.Steps.GroupBy(s => s.Order).Any(g => g.Count() > 1))
            throw new InvalidOperationException("Порядкові номери кроків не можуть повторюватися.");

        // Залишити можна лише зображення, яке вже належить крокам цього рецепту
        var existingImages = recipe.Steps
            .Where(s => s.ImageUrl is not null)
            .Select(s => s.ImageUrl!)
            .ToHashSet();

        foreach (var step in request.Steps)
        {
            if (step.Image is not null)
                EnsureValidImage(step.Image, $"Зображення кроку {step.Order} пошкоджене або має непідтримуваний формат.");
            else if (!string.IsNullOrEmpty(step.ImageUrl) && !existingImages.Contains(step.ImageUrl))
                throw new InvalidOperationException($"Зображення кроку {step.Order} не належить цьому рецепту.");
        }
    }

    // Кроки зіставляємо за Order: оновлюємо наявні, видаляємо зайві, додаємо нові
    private async Task ApplyStepsAsync(Recipe recipe, UpdateRecipeStepDto[] steps)
    {
        var requested = steps.ToDictionary(s => s.Order);

        foreach (var step in recipe.Steps.Where(s => !requested.ContainsKey(s.Order)).ToList())
            recipe.Steps.Remove(step);

        foreach (var dto in steps)
        {
            var imageUrl = dto.Image is not null
                ? await SaveImageAsync(dto.Image, StepImageWidths)
                : string.IsNullOrEmpty(dto.ImageUrl) ? null : dto.ImageUrl;

            var step = recipe.Steps.FirstOrDefault(s => s.Order == dto.Order);
            if (step is null)
            {
                step = new RecipeStep { Order = dto.Order, Description = dto.Description };
                recipe.Steps.Add(step);
            }

            step.Description = dto.Description;
            step.Timer = dto.Timer;
            step.ImageUrl = imageUrl;
        }
    }

    // RecipeProduct має складений ключ (RecipeId, ProductId), тому не перестворюємо наявні рядки
    private static void ApplyIngredients(Recipe recipe, ICollection<RecipeIngredientDto> ingredients)
    {
        var weights = ingredients.ToDictionary(i => i.ProductId, i => i.Weight);

        foreach (var rp in recipe.RecipeProducts.Where(rp => !weights.ContainsKey(rp.ProductId)).ToList())
            recipe.RecipeProducts.Remove(rp);

        foreach (var rp in recipe.RecipeProducts)
            rp.Weight = weights[rp.ProductId];

        var existingIds = recipe.RecipeProducts.Select(rp => rp.ProductId).ToHashSet();
        foreach (var (productId, weight) in weights.Where(w => !existingIds.Contains(w.Key)))
            recipe.RecipeProducts.Add(new RecipeProduct { ProductId = productId, Weight = weight });
    }

    private static void ApplyCategories(Recipe recipe, IReadOnlyList<RecipeCategory> categories)
    {
        var newIds = categories.Select(c => c.Id).ToHashSet();

        foreach (var category in recipe.RecipeCategories.Where(c => !newIds.Contains(c.Id)).ToList())
            recipe.RecipeCategories.Remove(category);

        var existingIds = recipe.RecipeCategories.Select(c => c.Id).ToHashSet();
        foreach (var category in categories.Where(c => !existingIds.Contains(c.Id)))
            recipe.RecipeCategories.Add(category);
    }

    private async Task<string> SaveImageAsync(byte[] bytes, int[] widths)
    {
        await using var ms = new MemoryStream(bytes);
        var result = await imageStorageService.SaveAsync(ms, widths);
        return result.VariantUrls[widths.Max()];
    }

    public async Task SaveRecipe(int recipeId, string userId)
    {
        var recipe = await repository.GetByIdAsync(recipeId) ?? throw new KeyNotFoundException();
        await repository.SaveRecipeAsync(recipe.Id, userId);
    }


    // Кроки, інгредієнти, категорії та збережені рецепти видаляються каскадно в БД
    public async Task DeleteAsync(int id, string? userId, bool isAdmin)
    {
        var recipe = await repository.GetByIdAsync(id) ?? throw new KeyNotFoundException("Рецепт не знайдено");
        EnsureCanModify(recipe, userId, isAdmin, "Ви можете видаляти лише власні рецепти.");

        if (await repository.HasDayPlanEntriesAsync(id))
            throw new InvalidOperationException("Неможливо видалити рецепт, який використовується в планах на день.");

        if (await repository.HasDiaryEntriesAsync(id))
            throw new InvalidOperationException("Неможливо видалити рецепт, який є в щоденнику харчування.");

        var images = CollectImageUrls(recipe);
        await repository.DeleteAsync(recipe);
        await imageCleanupService.DeleteUnusedAsync(images);
    }

    private static List<string?> CollectImageUrls(Recipe recipe)
        => [recipe.ImageUrl, ..recipe.Steps.Select(s => s.ImageUrl)];

    private static void EnsureCanModify(Recipe recipe, string? userId, bool isAdmin, string message)
    {
        if (!isAdmin && (recipe.UserId is null || recipe.UserId != userId))
            throw new UnauthorizedAccessException(message);
    }
}