using Domain.Enums;

namespace Application.Users.DTOs;

// required — System.Text.Json відхиляє запит без поля (ProblemDetails 400), а не підставляє 0
public class BodyParamsRequest
{
    public required Sex Sex { get; set; }
    public required DateOnly BirthDate { get; set; }
    public required decimal HeightCm { get; set; }
    public required decimal WeightKg { get; set; }
    public required ActivityLevel ActivityLevel { get; set; }
    public required WeightGoal WeightGoal { get; set; }
}
