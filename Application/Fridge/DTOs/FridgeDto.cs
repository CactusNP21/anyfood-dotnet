namespace Application.Fridge.DTOs;

public class FridgeDto
{
    public int Id { get; set; }
    public ICollection<FridgeItemDto> Items { get; set; } = [];
}

public class FridgeItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public float Weight { get; set; } // AP, як куплено
}