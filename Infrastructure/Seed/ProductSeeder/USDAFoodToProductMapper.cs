using Domain.Entities;

namespace Infrastructure.Seed.ProductSeeder;

public static class ProductMapper
{
    private const int CaloriesNutrientId = 1008; // Energy (kcal)
    private const int ProteinNutrientId = 1003;  // Protein
    private const int FatNutrientId = 1004;      // Total lipid (fat)
    private const int CarbsNutrientId = 1005;    // Carbohydrate, by difference

    public static Product ToProduct(this FoundationFoodRaw raw, Category? category = null)
    {
        decimal GetNutrient(int nutrientId)
        {
            return raw.FoodNutrients
                .FirstOrDefault(x => x.Nutrient?.Id == nutrientId)?
                .Amount ?? 0m;
        }

        return new Product
        {
            // Id -> do not set if EF generates it
            Name = raw.Description,
            Calories = GetNutrient(CaloriesNutrientId),
            Protein = GetNutrient(ProteinNutrientId),
            Fat = GetNutrient(FatNutrientId),
            Carbs = GetNutrient(CarbsNutrientId),

            GlycemicIndex = null,
            ImageUrl = null,

            // JSON does not contain price -> choose your default
            Price = 0m,

            // Since these are imported seed/system foods
            IsSystem = true,

            UserId = null,

            Categories = category is not null
                ? new List<Category> { category }
                : new List<Category>()
        };
    }
}