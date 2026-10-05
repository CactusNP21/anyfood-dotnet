namespace Application.FoodDiary.DTOs;

public class FoodDiaryEntryRequest
{
    public int? ProductId { get; set; }
    public int? RecipeId { get; set; }
    public float Weight { get; set; }
    public short Time { get; set; }
}
