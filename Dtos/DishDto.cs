using anyfood_dotnet.Models;

namespace anyfood_dotnet.Dtos;

public abstract class DishDTOBase
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string ImageUrl { get; set; }
}

public class DishCreateRequest: DishDTOBase
{
    public List<IngredientCreateRequest> Ingredients { get; set; }
}

public class DishResponse : DishDTOBase
{
    public int Id { get; set; }
    public required float Price { get; set; }
    public List<IngredientResponse> Ingredients { get; set; }
}