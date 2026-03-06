using anyfood_dotnet.Dtos;
using anyfood_dotnet.Models;
using Mapster;

namespace anyfood_dotnet.Mappings;

public class DishMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<DishCreateRequest, Models.Dish>()
            .Map(dest => dest.Ingredients, src => src.Ingredients.Select(i => new Ingredient
            {
                ProductId = i.ProductId,
                Weight = i.Weight
                // Id is not set — EF will generate it
            }).ToList());
    }
}