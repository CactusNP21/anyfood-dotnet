using API.Base;
using Application.RecipeCategories.DTOs;
using Application.RecipeCategories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/recipe-categories")]
public class RecipeCategoryController(IRecipeCategoryService service)
    : BaseController<
        IRecipeCategoryService,
        RecipeCategoryDto,
        RecipeCategoryCreateRequest,
        RecipeCategoryUpdateRequest>(service);