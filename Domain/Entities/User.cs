using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class User : IdentityUser
{
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ── Параметри тіла — для оцінки цілі калорій на фронті; null, поки юзер їх не зберіг ──
    public Sex? Sex { get; set; }
    public DateOnly? BirthDate { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? WeightKg { get; set; }
    public ActivityLevel? ActivityLevel { get; set; }
    public WeightGoal? WeightGoal { get; set; }
    public DateTimeOffset? BodyParamsUpdatedAt { get; set; }
}
