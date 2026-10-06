using Application.Users.DTOs;

namespace Application.Users.Interfaces;

public interface IUserService
{
    Task<UserProfileResponse> GetProfileAsync(string userId);
    Task<BodyParamsDto> UpdateBodyParamsAsync(string userId, BodyParamsRequest request);
    Task ClearBodyParamsAsync(string userId);
}
