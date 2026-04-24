using Application.Base.Interfaces;
using Application.RecipeCategories.DTOs;

namespace Application.RecipeCategories.Interfaces;

public interface IRecipeCategoryService: IBaseService<RecipeCategoryDto, RecipeCategoryCreateRequest, RecipeCategoryUpdateRequest>;