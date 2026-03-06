namespace anyfood_dotnet.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ImgUrl { get; set; } = String.Empty;
    public string Carbs { get; set; } = String.Empty;
    public string Fats { get; set; } =  String.Empty;
    public string Protein { get; set; } = String.Empty;
    public string Price { get; set; } = String.Empty;
}