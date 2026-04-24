using API.Base;
using Application.Recipes.DTOs;
using Application.Recipes.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/recipe")]
public class RecipeController(IRecipeService service): BaseController<IRecipeService, RecipeDto, CreateRecipeRequest, UpdateRecipeRequest>(service);