using anyfood_dotnet.Dtos;
using Mapster;

namespace anyfood_dotnet.Mappings;

public class MapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Models.Ingredient, IngredientDto>()
            .Map(dest => dest.CategoryName, src => src.Category.Name);
    }
}