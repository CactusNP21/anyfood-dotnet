using Domain.Enums;

namespace Application.Users.DTOs;

// Відповідь: усі поля null, поки юзер не зберіг параметри
public class BodyParamsDto
{
    public Sex? Sex { get; set; }
    public DateOnly? BirthDate { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? WeightKg { get; set; }
    public ActivityLevel? ActivityLevel { get; set; }
    public WeightGoal? WeightGoal { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
