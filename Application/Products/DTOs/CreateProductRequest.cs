using System.ComponentModel.DataAnnotations;

namespace Application.Products.DTOs;

public class CreateProductRequest
{
    [Required(ErrorMessage = "Назва категорії обов'язкова.")]
    [MinLength(2, ErrorMessage = "Назва має містити мінімум 2 символи.")]
    [MaxLength(100, ErrorMessage = "Назва не може перевищувати 100 символів.")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Калорії обов'язкові.")]
    public decimal Calories { get; set; }
    
    [Required]
    public decimal Protein { get; set; }
    [Required]
    public decimal Fat { get; set; }
    [Required]
    public decimal Carbs { get; set; }
    [Required]
    public decimal Price { get; set; }
    [Required]
    public int CategoryId { get; set; }
    [Required]
    public string GlycemicIndex { get; set; }
    [Required]
    public string ImageUrl { get; set; }

}