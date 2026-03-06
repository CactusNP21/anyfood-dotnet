using anyfood_dotnet.Base;
using anyfood_dotnet.Dtos;
using anyfood_dotnet.Models;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace anyfood_dotnet.Dish;

public class DishController(AppDbContext context)
    : ControllerRestBase<Models.Dish, DishResponse, DishCreateRequest>(context)
{
    [HttpGet]
    public override async Task<ActionResult<List<DishResponse>>> GetAll()
    {
        
        var dishes = await context.Dishes.Include(d => d.Ingredients)
            .ThenInclude(i => i.Product)
            .ToListAsync();
        
        var response = dishes.Select(d => new DishResponse
        {
            ImageUrl = d.ImageUrl,
            Description = d.Description,
            Id = d.Id,
            Name = d.Name,
            Ingredients = d.Ingredients.Select(i => new IngredientResponse
            {
                Name = i.Product?.Name,
                Weight = i.Weight,
                ProductId = i.ProductId
            }).ToList(),
            Price = d.Ingredients.Sum(i =>
                float.Parse(i.Weight, CultureInfo.InvariantCulture) / 100f * float.Parse(i.Product?.Price ?? "0", CultureInfo.InvariantCulture))
        }).ToList();

        return Ok(response);
    }

    [HttpPost] 
    public override async Task<ActionResult<DishResponse>> Create(DishCreateRequest request)
    {
        var dish = new Models.Dish
        {
            Name = request.Name,
            Description = request.Description,
            Ingredients = request.Ingredients.Select(i => new Ingredient
                {
                    ProductId = i.ProductId,
                    Weight = i.Weight
                })
                .ToList(),
            ImageUrl = request.ImageUrl
        };

        context.Dishes.Add(dish);
        await context.SaveChangesAsync();

        await context.Entry(dish)
            .Collection(d => d.Ingredients)
            .Query()
            .Include(i => i.Product)
            .LoadAsync();

        return Ok(dish.Adapt<DishResponse>());
    }
}
;