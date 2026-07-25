using Application.Categories.Interfaces;
using Application.Products.DTOs;
using Application.Products.Interfaces;
using Domain.Entities;
using Mapster;

namespace Application.Products.Services;

public class ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository) : IProductService
{
    public async Task<IReadOnlyList<ProductSummaryDto>> GetAllAsync()
    {
        var products = await productRepository.GetAllAsync();
        return products.Select(product => product.Adapt<ProductSummaryDto>()).ToList();
    }

    public async Task<IReadOnlyList<ProductSummaryDto>> FilterAsync(ProductFilterRequest filter)
    {
        var products = await productRepository.FilterAsync(filter);
        return products.Select(p => p.Adapt<ProductSummaryDto>()).ToList();
    }

    public async Task<ProductDto> GetByIdAsync(int id)
    {
        var product = await productRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Продукт не знайдено.");

        return product.Adapt<ProductDto>();
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, string userId, bool isSystem)
    {
        Product? parent = request.ParentProductId is int pid
            ? await productRepository.GetByIdAsync(pid)
              ?? throw new KeyNotFoundException("Батьківський продукт не знайдено.")
            : null;

        if (parent is null && (request.Protein is null || request.Fat is null
            || request.Carbs is null || request.Price is null))
            throw new InvalidOperationException("Кореневий продукт повинен мати всі значення БЖВ та ціну.");

        var protein = request.Protein ?? parent!.Protein;
        var fat = request.Fat ?? parent!.Fat;
        var carbs = request.Carbs ?? parent!.Carbs;
        var price = request.Price ?? parent!.Price;
        var glycemicIndex = request.GlycemicIndex ?? parent?.GlycemicIndex;
        var calories = CalculateCalories(carbs, fat, protein);

        var categories = await categoryRepository.GetByIdsAsync(request.CategoryIds);

        var product = new Product
        {
            Name = request.Name,
            OwnProtein = protein,
            OwnFat = fat,
            OwnCarbs = carbs,
            OwnCalories = calories,
            OwnPrice = price,
            OwnGlycemicIndex = glycemicIndex,
            Protein = protein,
            Fat = fat,
            Carbs = carbs,
            Calories = calories,
            Price = price,
            GlycemicIndex = glycemicIndex,
            ImageUrl = request.ImageUrl,
            IsSystem = isSystem,
            Categories = categories.ToList(),
            ParentProductId = parent?.Id,
            UserId = userId,
            EdiblePortionFactor = 1
        };

        var created = await productRepository.CreateAsync(product);

        created.Path = parent is null ? created.Id.ToString() : $"{parent.Path}.{created.Id}";
        await productRepository.UpdateAsync(created);

        if (parent is not null)
            await RecalculateChainAsync(parent.Id);

        return created.Adapt<ProductDto>();
    }

    public async Task<ProductDto> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = await productRepository.GetByIdWithChildrenAsync(id)
            ?? throw new KeyNotFoundException("Продукт не знайдено.");

        var isLeaf = product.Children.Count == 0;
        var wantsNutritionChange =
            request.Protein != product.OwnProtein || request.Fat != product.OwnFat ||
            request.Carbs != product.OwnCarbs || request.Price != product.OwnPrice ||
            request.GlycemicIndex != product.OwnGlycemicIndex;

        if (!isLeaf && wantsNutritionChange)
            throw new InvalidOperationException(
                "Неможливо редагувати БЖВ/ціну продукту з дочірніми продуктами — значення обчислюються автоматично.");

        var oldParentId = product.ParentProductId;
        var isReparenting = request.ParentProductId != product.ParentProductId;

        Product? newParent = null;
        if (isReparenting && request.ParentProductId is int newParentId)
        {
            newParent = await productRepository.GetByIdAsync(newParentId)
                ?? throw new KeyNotFoundException("Батьківський продукт не знайдено.");

            if (WouldCreateCycle(product, newParent))
                throw new InvalidOperationException("Продукт не може бути власним предком.");
        }

        product.Name = request.Name;
        product.ImageUrl = request.ImageUrl;
        product.Categories = (await categoryRepository.GetByIdsAsync(request.Categories)).ToList();

        if (isLeaf)
        {
            product.OwnProtein = request.Protein;
            product.OwnFat = request.Fat;
            product.OwnCarbs = request.Carbs;
            product.OwnPrice = request.Price;
            product.OwnCalories = CalculateCalories(request.Carbs, request.Fat, request.Protein);
            product.OwnGlycemicIndex = request.GlycemicIndex;

            product.Protein = product.OwnProtein;
            product.Fat = product.OwnFat;
            product.Carbs = product.OwnCarbs;
            product.Calories = product.OwnCalories;
            product.Price = product.OwnPrice;
            product.GlycemicIndex = product.OwnGlycemicIndex;
        }

        if (isReparenting)
        {
            product.ParentProductId = newParent?.Id;
            product.Path = newParent is null ? product.Id.ToString() : $"{newParent.Path}.{product.Id}";
        }

        var updated = await productRepository.UpdateAsync(product);

        if (isReparenting)
        {
            if (oldParentId is not null) await RecalculateChainAsync(oldParentId.Value);
            if (newParent is not null) await RecalculateChainAsync(newParent.Id);
        }
        else if (isLeaf && product.ParentProductId is not null)
        {
            await RecalculateChainAsync(product.ParentProductId.Value);
        }

        return updated.Adapt<ProductDto>();
    }

    public async Task DeleteAsync(int id)
    {
        var product = await productRepository.GetByIdWithChildrenAsync(id)
            ?? throw new KeyNotFoundException("Продукт не знайдено.");

        if (product.Children.Count > 0)
            throw new InvalidOperationException("Неможливо видалити продукт, який має дочірні продукти.");

        var hasRecipes = await productRepository.HasRecipesAsync(id);
        if (hasRecipes)
            throw new InvalidOperationException("Неможливо видалити продукт, який використовується в рецептах.");

        var parentId = product.ParentProductId;
        await productRepository.DeleteAsync(product);

        if (parentId is not null)
            await RecalculateChainAsync(parentId.Value);
    }

    // ── Private ──────────────────────────────────────────────────────────────

    private static bool WouldCreateCycle(Product product, Product newParent)
        => newParent.Path.Split('.').Contains(product.Id.ToString());

    // Перераховує вузол і йде вгору по ланцюгу предків
    private async Task RecalculateChainAsync(int nodeId)
    {
        int? currentId = nodeId;

        while (currentId is not null)
        {
            var node = await productRepository.GetByIdWithChildrenAsync(currentId.Value)
                ?? throw new KeyNotFoundException("Продукт не знайдено.");

            if (node.Children.Count > 0)
            {
                node.Protein = node.Children.Average(c => c.Protein);
                node.Fat = node.Children.Average(c => c.Fat);
                node.Carbs = node.Children.Average(c => c.Carbs);
                node.Calories = node.Children.Average(c => c.Calories);
                node.Price = node.Children.Average(c => c.Price);

                var giValues = node.Children
                    .Where(c => c.GlycemicIndex.HasValue)
                    .Select(c => c.GlycemicIndex!.Value)
                    .ToList();
                node.GlycemicIndex = giValues.Count > 0 ? (int)Math.Round(giValues.Average()) : null;
            }
            else
            {
                // дітей більше немає — повертаємось до власних (origin) значень
                node.Protein = node.OwnProtein;
                node.Fat = node.OwnFat;
                node.Carbs = node.OwnCarbs;
                node.Calories = node.OwnCalories;
                node.Price = node.OwnPrice;
                node.GlycemicIndex = node.OwnGlycemicIndex;
            }

            await productRepository.UpdateAsync(node);
            currentId = node.ParentProductId;
        }
    }

    private static decimal CalculateCalories(decimal carbs, decimal fat, decimal protein)
        => carbs * 4 + fat * 9 + protein * 4;
}