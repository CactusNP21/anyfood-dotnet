using Application.Auth.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Identity;

public class UserRepository : IUserRepository
{
    private readonly UserManager<User> userManager;

    public UserRepository(UserManager<User> userManager)
    {
        this.userManager = userManager;
    }

    public Task<User?> FindByUsernameAsync(string username)
        => userManager.FindByNameAsync(username);

    public Task<User?> FindByEmailAsync(string email)
        => userManager.FindByEmailAsync(email);

    public Task<User?> FindByRefreshTokenAsync(string refreshToken)
        => userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

    public Task<bool> CheckPasswordAsync(User user, string password)
        => userManager.CheckPasswordAsync(user, password);

    public async Task CreateAsync(User user, string password)
    {
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(errors);
        }
    }

    public async Task UpdateAsync(User user)
    {
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(errors);
        }
    }
}