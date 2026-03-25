using Application.Auth.Interfaces;
using Application.Users.DTOs;
using Application.Users.Interfaces;

namespace Application.Users.Services;

public class UserService : IUserService
{
    private readonly IUserRepository userRepository;

    public UserService(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task<UserProfileResponse> GetProfileAsync(string userId)
    {
        var user = await userRepository.FindByIdAsync(userId)
                   ?? throw new KeyNotFoundException("Користувача не знайдено.");

        return new UserProfileResponse
        {
            Id = user.Id,
            Name = user.Name,
            Username = user.UserName!,
            Email = user.Email!,
            AvatarUrl = user.AvatarUrl,
        };
    }
}