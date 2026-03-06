namespace Application.Auth.DTOs;

public class LoginResponse
{
    public required string AccessToken { get; set; } = string.Empty;
    public required string RefreshToken { get; set; } = string.Empty;
    public required int ExpiresIn { get; set; }
    public required UserDto User { get; set; } = null!;
}

public class UserDto
{
    public required string Id { get; set; } = string.Empty;
    public required string Name { get; set; } = string.Empty;
    public required string Username { get; set; } = string.Empty;
    public required string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}