namespace Domain.Entities;

public class FridgeItem
{
    public int Id { get; set; }

    public int FridgeId { get; set; }
    public Fridge Fridge { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    // Вага "як куплено" (AP) - з шкіркою, шкаралупою тощо
    public float Weight { get; set; }
}