namespace anyfood_dotnet.Dtos;

public class ProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Weight { get; set; } = string.Empty;
    public string ImgUrl { get; set; } = string.Empty;
    public required string Price { get; set; }
}

// What the client SENDS to create a product
public class ProductCreateRequest
{
    public string Name { get; set; }
    public string ImgUrl { get; set; } = String.Empty;
    public string Carbs { get; set; } = String.Empty;
    public string Fats { get; set; } =  String.Empty;
    public string Protein { get; set; } = String.Empty;
}

// What the client SENDS to update a product
public class ProductUpdateRequest
{
    public string Name { get; set; } = string.Empty;
}