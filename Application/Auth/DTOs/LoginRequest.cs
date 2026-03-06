using System.ComponentModel.DataAnnotations;

namespace Application.Auth.DTOs;

public class LoginRequest
{
    [Required]
    [MinLength(3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

}