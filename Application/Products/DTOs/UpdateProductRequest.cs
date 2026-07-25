using System.ComponentModel.DataAnnotations;

namespace Application.Products.DTOs;

public class UpdateProductRequest
{
    [Required] public int Id { get; set; }

    [Required(ErrorMessage = "Назва обов'язкова.")]
    [MinLength(2, ErrorMessage = "Назва має містити мінімум 2 символи.")]
    [MaxLength(100, ErrorMessage = "Назва не може перевищувати 100 символів.")]
    public string Name { get; set; } = string.Empty;

    [Required] public string ImageUrl { get; set; } = string.Empty;

    public int? ParentProductId { get; set; }

    [Required] public decimal Protein { get; set; }
    [Required] public decimal Fat { get; set; }
    [Required] public decimal Carbs { get; set; }
    [Required] public decimal Price { get; set; }
    public int? GlycemicIndex { get; set; }

    [Required] public int[] Categories { get; set; } = [];
}