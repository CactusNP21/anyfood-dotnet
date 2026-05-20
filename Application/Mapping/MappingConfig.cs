using Application.Recipes.DTOs;
using Application.Recipes.Models;
using Domain.Entities;
using Mapster;

namespace Application.Mapping;

public static class MappingConfig
{
    public static void Configure()
    {

        TypeAdapterConfig<Recipe, RecipeVersion>
            .NewConfig()
            .Map(dest => dest.VersionNumber, src => 0);
        
        
        TypeAdapterConfig<CreateRecipeRequest, Recipe>
            .NewConfig()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Calories)
            .Ignore(dest => dest.Protein)
            .Ignore(dest => dest.Fat)
            .Ignore(dest => dest.Carbs)
            .Ignore(dest => dest.Price)
            .Map(dest => dest.RecipeProducts, src => src.RecipeProducts.Select(rp => new RecipeProduct
            {
                ProductId = rp.ProductId,
                Weight = rp.Weight
            }).ToList());

        TypeAdapterConfig<NutritionPer100G, Recipe>
            .NewConfig()
            .Map(dest => dest.Calories, src => src.Calories)
            .Map(dest => dest.Protein,  src => src.Protein)
            .Map(dest => dest.Fat,      src => src.Fat)
            .Map(dest => dest.Carbs,    src => src.Carbs)
            .Map(dest => dest.Price,    src => src.Price)
            .IgnoreNonMapped(true); // не чіпати поля яких немає в NutritionPer100G

    }
}