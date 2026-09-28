using System.ComponentModel.DataAnnotations;

namespace Application.Products.DTOs;

public class CreateProductRequest
{
    [Required(ErrorMessage = "Назва обов'язкова.")]
    [MinLength(2, ErrorMessage = "Назва має містити мінімум 2 символи.")]
    [MaxLength(100, ErrorMessage = "Назва не може перевищувати 100 символів.")]
    public string Name { get; set; } = string.Empty;

    public int? ParentProductId { get; set; }

    // null = успадкувати від батька при створенні (лише для не-кореневих продуктів)
    public decimal? Protein { get; set; }
    public decimal? Fat { get; set; }
    public decimal? Carbs { get; set; }
    public decimal? Price { get; set; }
    public int? GlycemicIndex { get; set; }
    public decimal? Salt { get; set; }

    [Required] public int[] CategoryIds { get; set; } = [];
    [Required] public string ImageUrl { get; set; } = string.Empty;
}