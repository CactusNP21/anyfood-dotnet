using Domain.Entities;

namespace Application.Auth.Interfaces;

public interface IUserRepository
{
    Task<User?> FindByIdAsync(string id);
    Task<User?> FindByUsernameAsync(string username);
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByRefreshTokenAsync(string refreshToken);
    Task<bool> CheckPasswordAsync(User user, string password);
    Task CreateAsync(User user, string password);
    Task UpdateAsync(User user);
}