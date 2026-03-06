namespace anyfood_dotnet.Dtos;

public class IngredientCreateRequest
{
    public int ProductId { get; set; }
    public string Weight { get; set; }
}

public class IngredientResponse
{
    public string? Name { get; set; }
    public required int ProductId { get; set; }
    public string Weight { get; set; }
    public string ProductName { get; set; }
}