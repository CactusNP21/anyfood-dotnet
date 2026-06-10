using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;

namespace Infrastructure.Seed.ProductSeeder;

using System.Text.Json;
using Microsoft.EntityFrameworkCore;

public class ProductSeeder
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public ProductSeeder(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public async Task SeedProductsAsync()
    {
        var path = Path.Combine(
            _env.ContentRootPath,
            "Data",
            "FoodData_Central_foundation_food_json_2026-04-30.json");

        Console.WriteLine($"Path: {path}");

        if (!File.Exists(path))
        {
            Console.WriteLine("JSON file not found");
            return;
        }


        var json = await File.ReadAllTextAsync(path);

        var root = JsonSerializer.Deserialize<FoodDataRoot>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (root?.FoundationFoods is null || root.FoundationFoods.Count == 0)
            return;

        foreach (var raw in root.FoundationFoods)
        {
            // Prevent duplicates by name (or create a better unique rule)
            var exists = await _context.Products.AnyAsync(p => p.Name == raw.Description);
            if (exists)
                continue;

            Category? category = null;
            var categoryName = raw.FoodCategory?.Description?.Trim();

            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name == categoryName);

                if (category is null)
                {
                    category = new Category
                    {
                        Name = categoryName
                    };

                    _context.Categories.Add(category);
                }
            }

            var product = raw.ToProduct(category);
            _context.Products.Add(product);
        }

        await _context.SaveChangesAsync();
    }
}