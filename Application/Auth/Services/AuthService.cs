using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Auth.DTOs;
using Application.Auth.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Application.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository userRepository;
    private readonly IConfiguration configuration;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        this.userRepository = userRepository;
        this.configuration = configuration;
    }

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await userRepository.FindByUsernameAsync(request.Username);
        if (existingUser is not null)
            throw new InvalidOperationException("Користувач з таким іменем вже існує.");

        var existingEmail = await userRepository.FindByEmailAsync(request.Email);
        if (existingEmail is not null)
            throw new InvalidOperationException("Користувач з таким email вже існує.");

        var user = new User
        {
            Name = request.Name,
            UserName = request.Username,
            Email = request.Email,
        };

        await userRepository.CreateAsync(user, request.Password);

        return await GenerateLoginResponseAsync(user);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await userRepository.FindByUsernameAsync(request.Username);
        if (user is null || !await userRepository.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedAccessException("Невірне імʼя користувача або пароль.");

        return await GenerateLoginResponseAsync(user);
    }

    public async Task<LoginResponse> RefreshAsync(string refreshToken)
    {
        var user = await userRepository.FindByRefreshTokenAsync(refreshToken);
        if (user is null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Refresh token недійсний або прострочений.");

        return await GenerateLoginResponseAsync(user);
    }

    public async Task RevokeAsync(string refreshToken)
    {
        var user = await userRepository.FindByRefreshTokenAsync(refreshToken);
        if (user is null) return;

        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;
        await userRepository.UpdateAsync(user);
    }

    // ── Private ────────────────────────────────────────────────────────────────

    private async Task<LoginResponse> GenerateLoginResponseAsync(User user)
    {
        var roles = await userRepository.GetRolesAsync(user);
        var accessToken = await GenerateAccessToken(user, roles);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(
            int.Parse(configuration["Jwt:RefreshTokenExpirationDays"]!));

        await userRepository.UpdateAsync(user);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = int.Parse(configuration["Jwt:ExpirationMinutes"]!) * 60,
            User = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Username = user.UserName!,
                Email = user.Email!,
                AvatarUrl = user.AvatarUrl,
                Roles = roles
            }
        };
    }

    private async Task<string> GenerateAccessToken(User user, IList<string> roles)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!));

        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = user.Id,
            [JwtRegisteredClaimNames.UniqueName] = user.UserName!,
            [JwtRegisteredClaimNames.Email] = user.Email!,
            [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
            [ClaimTypes.Role] = roles.ToArray()
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Claims = claims,
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            Expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpirationMinutes"]!)),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    private static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}