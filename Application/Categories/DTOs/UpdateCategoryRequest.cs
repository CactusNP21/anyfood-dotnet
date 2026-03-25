using System.ComponentModel.DataAnnotations;

namespace Application.Categories.DTOs;

public class UpdateCategoryRequest
{
    [Required(ErrorMessage = "Назва категорії обов'язкова.")]
    [MinLength(2, ErrorMessage = "Назва має містити мінімум 2 символи.")]
    [MaxLength(100, ErrorMessage = "Назва не може перевищувати 100 символів.")]
    public string Name { get; set; } = string.Empty;
}