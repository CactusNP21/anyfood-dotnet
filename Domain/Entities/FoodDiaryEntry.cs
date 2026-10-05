namespace Domain.Entities;

public class FoodDiaryEntry
{
    public int Id { get; set; }

    public int FoodDiaryDayId { get; set; }
    public FoodDiaryDay FoodDiaryDay { get; set; } = null!;

    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    public int? RecipeId { get; set; }
    public Recipe? Recipe { get; set; }

    // Вага в грамах
    public float Weight { get; set; }
    public short Time { get; set; }
}
