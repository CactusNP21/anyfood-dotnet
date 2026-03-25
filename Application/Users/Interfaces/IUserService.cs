using Application.Users.DTOs;

namespace Application.Users.Interfaces;

public interface IUserService
{
    Task<UserProfileResponse> GetProfileAsync(string userId);
}