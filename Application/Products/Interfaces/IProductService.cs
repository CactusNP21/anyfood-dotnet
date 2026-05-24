using Application.Products.DTOs;

namespace Application.Products.Interfaces;

public interface IProductService
{
    Task<IReadOnlyList<ProductSummaryDto>> GetAllAsync();
    Task<ProductDto> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(CreateProductRequest request);
    Task<ProductDto> UpdateAsync(int id, UpdateProductRequest request);
    Task<IReadOnlyList<ProductSummaryDto>> FilterAsync(ProductFilterRequest filter);
    Task DeleteAsync(int id);
}