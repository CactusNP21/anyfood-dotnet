namespace Infrastructure.Seed.ProductSeeder;

using System.Text.Json.Serialization;

public class FoodDataRoot
{
    [JsonPropertyName("FoundationFoods")]
    public List<FoundationFoodRaw> FoundationFoods { get; set; } = [];
}

public class FoundationFoodRaw
{
    [JsonPropertyName("fdcId")]
    public int FdcId { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("foodCategory")]
    public FoodCategoryRaw? FoodCategory { get; set; }

    [JsonPropertyName("foodNutrients")]
    public List<FoodNutrientRaw> FoodNutrients { get; set; } = [];
}

public class FoodCategoryRaw
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public class FoodNutrientRaw
{
    [JsonPropertyName("amount")]
    public decimal? Amount { get; set; }

    [JsonPropertyName("nutrient")]
    public NutrientRaw? Nutrient { get; set; }
}

public class NutrientRaw
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("unitName")]
    public string UnitName { get; set; } = string.Empty;
}