using Application.Products.DTOs;
using Application.Products.Interfaces;
using Domain.Entities;
using Mapster;

namespace Application.Products.Services;

public class ProductService(IProductRepository productRepository): IProductService
{
    public async Task<IReadOnlyList<ProductSummaryDto>> GetAllAsync()
    {
        var products = await productRepository.GetAllAsync();
        return products.Select(product => product.Adapt<ProductSummaryDto>()).ToList();
    }

    public async Task<ProductDto> GetByIdAsync(int id)
    {
        var product = await productRepository.GetByIdAsync(id)
                       ?? throw new KeyNotFoundException("Категорію не знайдено.");

        return product.Adapt<ProductDto>();
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request)
    {
        var created = await productRepository.CreateAsync(request.Adapt<Product>());

        return created.Adapt<ProductDto>();
    }

    public async Task<ProductDto> UpdateAsync(int id, UpdateProductRequest request)
    {
        var updated = await productRepository.UpdateAsync(request.Adapt<Product>());
        return updated.Adapt<ProductDto>();
    }

    public Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }
}