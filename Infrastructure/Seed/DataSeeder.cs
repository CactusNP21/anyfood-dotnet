using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await SeedCategoriesAsync(db);
        await SeedRecipeCategoriesAsync(db);
        await SeedSystemProductsAsync(db);
    }

    private static async Task SeedCategoriesAsync(AppDbContext db)
    {
        if (await db.Categories.AnyAsync()) return;

        var categories = new List<Category>
        {
            new() { Name = "Фрукти" },
            new() { Name = "Овочі" },
            new() { Name = "Молочні продукти та яйця" },
            new() { Name = "М'ясо та птиця" },
            new() { Name = "Зернові та крупи" },
            new() { Name = "Морепродукти" },
            new() { Name = "Бобові" },
            new() { Name = "Олії та жири" },
        };

        db.Categories.AddRange(categories);
        await db.SaveChangesAsync();
    }

    private static async Task SeedRecipeCategoriesAsync(AppDbContext db)
    {
        if (await db.RecipeCategories.AnyAsync()) return;

        var recipeCategories = new List<Domain.Entities.RecipeCategory>
        {
            new() { Name = "Сніданок" },
            new() { Name = "Обід" },
            new() { Name = "Вечеря" },
            new() { Name = "Перекус" },
            new() { Name = "Десерт" },
            new() { Name = "Суп" },
        };

        db.RecipeCategories.AddRange(recipeCategories);
        await db.SaveChangesAsync();
    }

    private static async Task SeedSystemProductsAsync(AppDbContext db)
    {
        if (await db.Products.AnyAsync(p => p.IsSystem)) return;

        var categories = await db.Categories.ToDictionaryAsync(c => c.Name, c => c);

        var products = new List<Product>
        {
            new()
            {
                Name = "Куряча грудка", IsSystem = true,
                Calories = 165, Protein = 31, Fat = 3.6m, Carbs = 0, GlycemicIndex = 0,
                Price = 5.99m, Categories = [categories["М'ясо та птиця"]]
            },
            new()
            {
                Name = "Яйце куряче", IsSystem = true,
                Calories = 155, Protein = 13, Fat = 11, Carbs = 1.1m, GlycemicIndex = 0,
                Price = 0.25m, Categories = [categories["Молочні продукти та яйця"]]
            },
            new()
            {
                Name = "Молоко незбиране", IsSystem = true,
                Calories = 61, Protein = 3.2m, Fat = 3.3m, Carbs = 4.8m, GlycemicIndex = 40,
                Price = 0.89m, Categories = [categories["Молочні продукти та яйця"]]
            },
            new()
            {
                Name = "Рис білий", IsSystem = true,
                Calories = 130, Protein = 2.7m, Fat = 0.3m, Carbs = 28.2m, GlycemicIndex = 72,
                Price = 0.60m, Categories = [categories["Зернові та крупи"]]
            },
            new()
            {
                Name = "Вівсянка", IsSystem = true,
                Calories = 389, Protein = 17, Fat = 7, Carbs = 66, GlycemicIndex = 55,
                Price = 0.40m, Categories = [categories["Зернові та крупи"]]
            },
            new()
            {
                Name = "Банан", IsSystem = true,
                Calories = 89, Protein = 1.1m, Fat = 0.3m, Carbs = 23, GlycemicIndex = 51,
                Price = 0.30m, Categories = [categories["Фрукти"]]
            },
            new()
            {
                Name = "Яблуко", IsSystem = true,
                Calories = 52, Protein = 0.3m, Fat = 0.2m, Carbs = 14, GlycemicIndex = 36,
                Price = 0.45m, Categories = [categories["Фрукти"]]
            },
            new()
            {
                Name = "Броколі", IsSystem = true,
                Calories = 34, Protein = 2.8m, Fat = 0.4m, Carbs = 7, GlycemicIndex = 15,
                Price = 1.20m, Categories = [categories["Овочі"]]
            },
            new()
            {
                Name = "Помідор", IsSystem = true,
                Calories = 18, Protein = 0.9m, Fat = 0.2m, Carbs = 3.9m, GlycemicIndex = 15,
                Price = 0.80m, Categories = [categories["Овочі"]]
            },
            new()
            {
                Name = "Картопля", IsSystem = true,
                Calories = 77, Protein = 2, Fat = 0.1m, Carbs = 17, GlycemicIndex = 78,
                Price = 0.35m, Categories = [categories["Овочі"]]
            },
            new()
            {
                Name = "Лосось", IsSystem = true,
                Calories = 208, Protein = 20, Fat = 13, Carbs = 0, GlycemicIndex = 0,
                Price = 8.99m, Categories = [categories["Морепродукти"]]
            },
            new()
            {
                Name = "Олія оливкова", IsSystem = true,
                Calories = 884, Protein = 0, Fat = 100, Carbs = 0, GlycemicIndex = 0,
                Price = 3.50m, Categories = [categories["Олії та жири"]]
            },
            new()
            {
                Name = "Йогурт грецький", IsSystem = true,
                Calories = 59, Protein = 10, Fat = 0.4m, Carbs = 3.6m, GlycemicIndex = 11,
                Price = 1.50m, Categories = [categories["Молочні продукти та яйця"]]
            },
            new()
            {
                Name = "Сочевиця червона", IsSystem = true,
                Calories = 116, Protein = 9, Fat = 0.4m, Carbs = 20, GlycemicIndex = 21,
                Price = 0.70m, Categories = [categories["Бобові"]]
            },
            new()
            {
                Name = "Хліб цільнозерновий", IsSystem = true,
                Calories = 247, Protein = 13, Fat = 3.4m, Carbs = 41, GlycemicIndex = 69,
                Price = 1.99m, Categories = [categories["Зернові та крупи"]]
            },
        };

        db.Products.AddRange(products);
        await db.SaveChangesAsync();
    }
}
