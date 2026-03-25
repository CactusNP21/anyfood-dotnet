namespace Application.Users.DTOs;

public class UserProfileResponse
{
    public required string Id { get; set; } = string.Empty;
    public required string Name { get; set; } = string.Empty;
    public required string Username { get; set; } = string.Empty;
    public required string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}