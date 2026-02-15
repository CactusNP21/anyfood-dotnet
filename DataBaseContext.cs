using anyfood_dotnet.Models;

namespace anyfood_dotnet;

using Microsoft.EntityFrameworkCore;

public class DataBaseContext(DbContextOptions<DataBaseContext> options) : DbContext(options)
{
    public DbSet<Models.Ingredient> ingredients { get; set; }
    
    // ✅ ДОДАЙТЕ НОВИЙ DbSet ДЛЯ КАТЕГОРІЙ
    public DbSet<Category> Categories { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ✅ КРОК 1: Заповнюємо таблицю категорій
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Фрукти" },
            new Category { Id = 2, Name = "Овочі" },
            new Category { Id = 3, Name = "М'ясо" },
            new Category { Id = 4, Name = "Риба" },
            new Category { Id = 5, Name = "Крупи" }
        );

        // ✅ КРОК 2: Оновлюємо дані для таблиці їжі, використовуючи CategoryId
        modelBuilder.Entity<Models.Ingredient>().HasData(
            // Тепер замість Category = "Фрукти" ми пишемо CategoryId = 1
            new Models.Ingredient { Id = 1, Name = "Яблуко", CategoryId = 1, ImageUrl = "", Calories = 52, Protein = 0.3, Fat = 0.2, Carbs = 14 },
            new Models.Ingredient { Id = 2, Name = "Банан", CategoryId = 1, ImageUrl = "", Calories = 89, Protein = 1.1, Fat = 0.3, Carbs = 23 },
            new Models.Ingredient { Id = 3, Name = "Куряча грудка", CategoryId = 3, ImageUrl = "", Calories = 165, Protein = 31, Fat = 3.6, Carbs = 0 },
            new Models.Ingredient { Id = 4, Name = "Лосось", CategoryId = 4, ImageUrl = "", Calories = 208, Protein = 20, Fat = 13, Carbs = 0 },
            new Models.Ingredient { Id = 5, Name = "Броколі", CategoryId = 2, ImageUrl = "", Calories = 55, Protein = 3.7, Fat = 0.6, Carbs = 11 },
            new Models.Ingredient { Id = 6, Name = "Рис білий", CategoryId = 5, ImageUrl = "", Calories = 130, Protein = 2.7, Fat = 0.3, Carbs = 28 },
            new Models.Ingredient { Id = 7, Name = "Авокадо", CategoryId = 1, ImageUrl = "", Calories = 160, Protein = 2, Fat = 15, Carbs = 9 }
        );
    }
}