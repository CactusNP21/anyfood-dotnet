using Application.Base.Interfaces;
using Domain.Entities;

namespace Application.RecipeCategories.Interfaces;

public interface IRecipeCategoryRepository : IBaseRepository<RecipeCategory>
{
    Task<IReadOnlyList<RecipeCategory>> GetByBatchIdAsync(List<int> categoryId);
};