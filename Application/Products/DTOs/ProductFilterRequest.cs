namespace Application.Products.DTOs;

public class ProductFilterRequest
{
    public ICollection<int>? CategoryIds { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Name { get; set; }
    public bool? IsSystem { get; set; }

}