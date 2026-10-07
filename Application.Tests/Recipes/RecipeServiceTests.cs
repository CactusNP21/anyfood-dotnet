using Application.Images.Interfaces;
using Application.Products.DTOs;
using Application.Products.Interfaces;
using Application.RecipeCategories.Interfaces;
using Application.Recipes.DTOs;
using Application.Recipes.Interfaces;
using Application.Recipes.Services;
using Domain.Entities;

namespace Application.Tests.Recipes;

public class RecipeServiceTests
{
    private const string OwnerId = "owner";
    private const string MainImage = "/images/main/1200.webp";
    private const string StepImage = "/images/step1/500.webp";

    // Валідний PNG 1x1
    private static readonly byte[] Png = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8DwHwAFBQIAX8jx0gAAAABJRU5ErkJggg==");

    private readonly FakeImageStorage images = new();
    private readonly FakeProductRepository products = new();
    private readonly FakeRecipeRepository recipes;
    private readonly FakeRecipeCategoryRepository categories = new();
    private readonly FakeImageCleanup cleanup = new();
    private readonly RecipeService service;

    public RecipeServiceTests()
    {
        recipes = new FakeRecipeRepository(products);
        cleanup.Recipes = recipes;
        products.Items.Add(NewProduct(1, calories: 100, salt: 1));
        products.Items.Add(NewProduct(2, calories: 300, salt: 3));
        categories.Items.Add(new RecipeCategory { Id = 10, Name = "Супи" });
        categories.Items.Add(new RecipeCategory { Id = 11, Name = "Десерти" });

        recipes.Recipe = new Recipe
        {
            Id = 5,
            Name = "Old",
            Price = 0,
            ImageUrl = MainImage,
            Portions = 1,
            IsSystem = false,
            UserId = OwnerId,
            RecipeProducts = [new RecipeProduct { RecipeId = 5, ProductId = 1, Weight = 100, Product = products.Items[0] }],
            RecipeCategories = [categories.Items[0]],
            Steps =
            [
                new RecipeStep { Id = 1, RecipeId = 5, Order = 1, Description = "step 1", ImageUrl = StepImage },
                new RecipeStep { Id = 2, RecipeId = 5, Order = 2, Description = "step 2" },
            ],
        };

        service = new RecipeService(recipes, images, products, categories, cleanup);
    }

    [Fact]
    public async Task Update_NewStepWithFile_KeepsMainAndExistingStepImagesAndUploadsNewOne()
    {
        var request = ValidRequest();
        request.Steps =
        [
            new UpdateRecipeStepDto { Order = 1, Description = "step 1 edited", ImageUrl = StepImage },
            new UpdateRecipeStepDto { Order = 2, Description = "step 2" },
            new UpdateRecipeStepDto { Order = 3, Description = "step 3", Timer = 60, Image = Png },
        ];

        var result = await service.UpdateAsync(5, request, isAdmin: false);

        Assert.Equal(MainImage, result.ImageUrl);
        Assert.Equal(1, images.SaveCount);
        Assert.Equal(3, result.Steps.Count);
        Assert.Equal(StepImage, result.Steps.ElementAt(0).ImageUrl);
        Assert.Equal("step 1 edited", result.Steps.ElementAt(0).Description);
        Assert.Null(result.Steps.ElementAt(1).ImageUrl);
        Assert.Equal("/images/saved1/500.webp", result.Steps.ElementAt(2).ImageUrl);
        Assert.Equal(60, result.Steps.ElementAt(2).Timer);
        Assert.Equal(1, recipes.SaveCount);
    }

    [Fact]
    public async Task Update_NewMainImage_ReplacesImageUrl()
    {
        var request = ValidRequest();
        request.Image = Png;

        var result = await service.UpdateAsync(5, request, isAdmin: false);

        Assert.Equal("/images/saved1/1200.webp", result.ImageUrl);
    }

    [Fact]
    public async Task Update_StepWithoutImageOrUrl_RemovesStepImage()
    {
        var request = ValidRequest();
        request.Steps = [new UpdateRecipeStepDto { Order = 1, Description = "step 1" }];

        var result = await service.UpdateAsync(5, request, isAdmin: false);

        var step = Assert.Single(result.Steps);
        Assert.Null(step.ImageUrl);
    }

    [Fact]
    public async Task Update_ForeignStepImageUrl_ThrowsAndSavesNothing()
    {
        var request = ValidRequest();
        request.Steps = [new UpdateRecipeStepDto { Order = 1, Description = "x", ImageUrl = "/images/other/500.webp" }];

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(5, request, isAdmin: false));
        Assert.Equal(0, recipes.SaveCount);
    }

    [Fact]
    public async Task Update_DuplicateStepOrder_Throws()
    {
        var request = ValidRequest();
        request.Steps =
        [
            new UpdateRecipeStepDto { Order = 1, Description = "a" },
            new UpdateRecipeStepDto { Order = 1, Description = "b" },
        ];

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(5, request, isAdmin: false));
    }

    [Fact]
    public async Task Update_NotOwner_ThrowsUnauthorized()
    {
        var request = ValidRequest();
        request.UserId = "someone-else";

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.UpdateAsync(5, request, isAdmin: false));
    }

    [Fact]
    public async Task Update_AdminNotOwner_Succeeds()
    {
        var request = ValidRequest();
        request.UserId = "admin";

        var result = await service.UpdateAsync(5, request, isAdmin: true);

        Assert.Equal("New", result.Name);
    }

    [Fact]
    public async Task Update_MissingRecipe_ThrowsKeyNotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateAsync(99, ValidRequest(), isAdmin: false));
    }

    [Fact]
    public async Task Update_ReplacesIngredientsAndCategoriesAndRecalculatesNutrition()
    {
        var request = ValidRequest();
        request.RecipeProducts =
        [
            new RecipeIngredientDto { ProductId = 1, Weight = 50 },
            new RecipeIngredientDto { ProductId = 2, Weight = 50 },
        ];
        request.RecipeCategories = [new() { Id = 11, Name = "Десерти" }];

        await service.UpdateAsync(5, request, isAdmin: false);

        var recipe = recipes.Recipe!;
        Assert.Equal([1, 2], recipe.RecipeProducts.Select(rp => rp.ProductId).Order());
        Assert.Equal(50, recipe.RecipeProducts.Single(rp => rp.ProductId == 1).Weight);
        Assert.Equal(11, Assert.Single(recipe.RecipeCategories).Id);
        Assert.Equal(200, recipe.Calories, 3);
        Assert.Equal(2, recipe.Salt, 3);
    }

    [Fact]
    public async Task Update_DuplicateProduct_Throws()
    {
        var request = ValidRequest();
        request.RecipeProducts =
        [
            new RecipeIngredientDto { ProductId = 1, Weight = 50 },
            new RecipeIngredientDto { ProductId = 1, Weight = 20 },
        ];

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(5, request, isAdmin: false));
    }

    [Fact]
    public async Task Delete_Owner_DeletesRecipe()
    {
        await service.DeleteAsync(5, OwnerId, isAdmin: false);

        Assert.True(recipes.Deleted);
    }

    [Fact]
    public async Task Delete_AdminNotOwner_DeletesRecipe()
    {
        await service.DeleteAsync(5, "admin", isAdmin: true);

        Assert.True(recipes.Deleted);
    }

    [Fact]
    public async Task Delete_NotOwner_ThrowsUnauthorized()
    {
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.DeleteAsync(5, "someone-else", isAdmin: false));
        Assert.False(recipes.Deleted);
    }

    [Fact]
    public async Task Delete_MissingRecipe_ThrowsKeyNotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteAsync(99, OwnerId, isAdmin: false));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task Delete_UsedInDayPlanOrDiary_ThrowsAndKeepsRecipe(bool inDayPlan, bool inDiary)
    {
        recipes.InDayPlan = inDayPlan;
        recipes.InDiary = inDiary;

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(5, OwnerId, isAdmin: false));
        Assert.False(recipes.Deleted);
    }

    [Fact]
    public async Task Update_PassesPreviousImagesToCleanupAfterSave()
    {
        var request = ValidRequest();
        request.Image = Png;

        await service.UpdateAsync(5, request, isAdmin: false);

        var call = Assert.Single(cleanup.Calls);
        Assert.Equal([MainImage, StepImage, null], call.Urls);
        Assert.Equal(1, call.SaveCountAtCall); // уже після збереження
    }

    [Fact]
    public async Task Update_ValidationFails_DoesNotCleanUp()
    {
        var request = ValidRequest();
        request.RecipeProducts = [];

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(5, request, isAdmin: false));
        Assert.Empty(cleanup.Calls);
    }

    [Fact]
    public async Task Delete_PassesAllRecipeImagesToCleanupAfterDelete()
    {
        await service.DeleteAsync(5, OwnerId, isAdmin: false);

        var call = Assert.Single(cleanup.Calls);
        Assert.Equal([MainImage, StepImage, null], call.Urls);
        Assert.True(call.DeletedAtCall);
    }

    [Fact]
    public async Task Delete_Blocked_DoesNotCleanUp()
    {
        recipes.InDiary = true;

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(5, OwnerId, isAdmin: false));
        Assert.Empty(cleanup.Calls);
    }

    private static UpdateRecipeRequest ValidRequest() => new()
    {
        Name = "New",
        RecipeProducts = [new RecipeIngredientDto { ProductId = 1, Weight = 100 }],
        RecipeCategories = [new() { Id = 10, Name = "Супи" }],
        Portions = 2,
        Steps =
        [
            new UpdateRecipeStepDto { Order = 1, Description = "step 1", ImageUrl = StepImage },
            new UpdateRecipeStepDto { Order = 2, Description = "step 2" },
        ],
        UserId = OwnerId,
    };

    private static Product NewProduct(int id, decimal calories, decimal salt) => new()
    {
        Id = id,
        Name = $"P{id}",
        Calories = calories,
        Protein = 0,
        Fat = 0,
        Carbs = 0,
        Salt = salt,
        Price = 0,
        EdiblePortionFactor = 1,
    };

    // ── Fakes ────────────────────────────────────────────────────────────────

    private class FakeRecipeRepository(FakeProductRepository products) : IRecipeRepository
    {
        public Recipe? Recipe { get; set; }
        public int SaveCount { get; private set; }
        public bool InDayPlan { get; set; }
        public bool InDiary { get; set; }
        public bool Deleted { get; private set; }

        public Task<bool> HasDayPlanEntriesAsync(int id) => Task.FromResult(InDayPlan);
        public Task<bool> HasDiaryEntriesAsync(int id) => Task.FromResult(InDiary);

        public Task DeleteAsync(Recipe category)
        {
            Deleted = true;
            return Task.CompletedTask;
        }

        public Task<Recipe?> GetByIdAsync(int id) => Task.FromResult(Recipe?.Id == id ? Recipe : null);

        public Task SaveChangesAsync()
        {
            SaveCount++;

            // Як EF після повторного запиту: підтягуємо Product для нових інгредієнтів
            foreach (var rp in Recipe!.RecipeProducts)
                rp.Product ??= products.Items.Single(p => p.Id == rp.ProductId);

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Recipe>> GetAllAsync() => throw new NotImplementedException();
        public Task<Recipe> CreateAsync(Recipe category) => throw new NotImplementedException();
        public Task UpdateAsync(Recipe category) => throw new NotImplementedException();
        public Task SaveRecipeAsync(int recipeId, string userId) => throw new NotImplementedException();
        public Task<Recipe> CreateRecipeAsync(Recipe recipe) => throw new NotImplementedException();
    }

    private class FakeImageStorage : IImageStorageService
    {
        public int SaveCount { get; private set; }

        public Task<ImageUploadResult> SaveAsync(Stream input, int[] widths, CancellationToken ct = default)
        {
            var hash = $"saved{++SaveCount}";
            var urls = widths.ToDictionary(w => w, w => $"/images/{hash}/{w}.webp");
            return Task.FromResult(new ImageUploadResult(hash, urls));
        }

        public Task<ImageUploadResult> SaveAsync(Stream input, CancellationToken ct = default) => throw new NotImplementedException();
        public Task DeleteAsync(string hash, CancellationToken ct = default) => throw new NotImplementedException();
        public string Enqueue(byte[] bytes, Func<string, IServiceProvider, CancellationToken, Task> onProcessed) => throw new NotImplementedException();
    }

    private class FakeImageCleanup : IImageCleanupService
    {
        public FakeRecipeRepository? Recipes { get; set; }
        public List<(List<string?> Urls, int SaveCountAtCall, bool DeletedAtCall)> Calls { get; } = [];

        public Task DeleteUnusedAsync(IEnumerable<string?> imageUrls)
        {
            Calls.Add((imageUrls.ToList(), Recipes?.SaveCount ?? -1, Recipes?.Deleted ?? false));
            return Task.CompletedTask;
        }
    }

    private class FakeProductRepository : IProductRepository
    {
        public List<Product> Items { get; } = [];

        public Task<IReadOnlyList<Product>> GetByBatchIdAsync(List<int> ids)
            => Task.FromResult<IReadOnlyList<Product>>(Items.Where(p => ids.Contains(p.Id)).ToList());

        public Task<IReadOnlyList<Product>> GetAllAsync() => throw new NotImplementedException();
        public Task<IReadOnlyList<Product>> FilterAsync(ProductFilterRequest filter) => throw new NotImplementedException();
        public Task<Product?> GetByIdAsync(int id) => throw new NotImplementedException();
        public Task<Product?> GetByIdWithChildrenAsync(int id) => throw new NotImplementedException();
        public Task<Product?> GetByNameAsync(string name) => throw new NotImplementedException();
        public Task<Product> CreateAsync(Product product) => throw new NotImplementedException();
        public Task<Product> UpdateAsync(Product product) => throw new NotImplementedException();
        public Task DeleteAsync(Product product) => throw new NotImplementedException();
        public Task<bool> HasRecipesAsync(int id) => throw new NotImplementedException();
        public Task<bool> HasDiaryEntriesAsync(int id) => throw new NotImplementedException();
    }

    private class FakeRecipeCategoryRepository : IRecipeCategoryRepository
    {
        public List<RecipeCategory> Items { get; } = [];

        public Task<IReadOnlyList<RecipeCategory>> GetByBatchIdAsync(List<int> ids)
            => Task.FromResult<IReadOnlyList<RecipeCategory>>(Items.Where(c => ids.Contains(c.Id)).ToList());

        public Task<IReadOnlyList<RecipeCategory>> GetAllAsync() => throw new NotImplementedException();
        public Task<RecipeCategory?> GetByIdAsync(int id) => throw new NotImplementedException();
        public Task<RecipeCategory> CreateAsync(RecipeCategory category) => throw new NotImplementedException();
        public Task UpdateAsync(RecipeCategory category) => throw new NotImplementedException();
        public Task DeleteAsync(RecipeCategory category) => throw new NotImplementedException();
    }
}
