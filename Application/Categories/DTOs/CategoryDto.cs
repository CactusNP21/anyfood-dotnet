namespace Application.Categories.DTOs;

public class CategoryDto
{
    public required int Id { get; set; }
    public required string Name { get; set; } = string.Empty;
    public required int ProductCount { get; set; }

}