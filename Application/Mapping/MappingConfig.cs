using Domain.Entities;
using Mapster;

namespace Application.Mapping;

public static class MappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<Product, ProductVersion>
            .NewConfig()
            .Map(dest => dest.VersionNumber, src => 0)
            .Map(dest => dest.Product, src => src);

        TypeAdapterConfig<Recipe, RecipeVersion>
            .NewConfig()
            .Map(dest => dest.VersionNumber, src => 0)
            .Map(dest => dest.Recipe, src => src);
    }
}