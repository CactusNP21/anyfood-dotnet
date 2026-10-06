using Application.Auth.Interfaces;
using Application.Users.DTOs;
using Application.Users.Interfaces;
using Domain.Entities;

namespace Application.Users.Services;

public class UserService : IUserService
{
    // Діапазони збігаються з isValidBodyParams на фронті — змінювати разом
    private const int MinAge = 15;
    private const int MaxAge = 100;
    private const decimal MinHeightCm = 100;
    private const decimal MaxHeightCm = 250;
    private const decimal MinWeightKg = 30;
    private const decimal MaxWeightKg = 300;

    private readonly IUserRepository userRepository;
    private readonly TimeProvider timeProvider;

    public UserService(IUserRepository userRepository, TimeProvider timeProvider)
    {
        this.userRepository = userRepository;
        this.timeProvider = timeProvider;
    }

    public async Task<UserProfileResponse> GetProfileAsync(string userId)
    {
        var user = await FindUserAsync(userId);

        return new UserProfileResponse
        {
            Id = user.Id,
            Name = user.Name,
            Username = user.UserName!,
            Email = user.Email!,
            AvatarUrl = user.AvatarUrl,
            BodyParams = ToBodyParamsDto(user),
        };
    }

    public async Task<BodyParamsDto> UpdateBodyParamsAsync(string userId, BodyParamsRequest request)
    {
        var now = timeProvider.GetUtcNow();
        ValidateBodyParams(request, DateOnly.FromDateTime(now.UtcDateTime));

        var user = await FindUserAsync(userId);

        user.Sex = request.Sex;
        user.BirthDate = request.BirthDate;
        user.HeightCm = Math.Round(request.HeightCm, 1);
        user.WeightKg = Math.Round(request.WeightKg, 1);
        user.ActivityLevel = request.ActivityLevel;
        user.WeightGoal = request.WeightGoal;
        user.BodyParamsUpdatedAt = now;

        await userRepository.UpdateAsync(user);
        return ToBodyParamsDto(user);
    }

    public async Task ClearBodyParamsAsync(string userId)
    {
        var user = await FindUserAsync(userId);

        user.Sex = null;
        user.BirthDate = null;
        user.HeightCm = null;
        user.WeightKg = null;
        user.ActivityLevel = null;
        user.WeightGoal = null;
        user.BodyParamsUpdatedAt = null;

        await userRepository.UpdateAsync(user);
    }

    // ── Private ──────────────────────────────────────────────────────────────

    private async Task<User> FindUserAsync(string userId)
        => await userRepository.FindByIdAsync(userId)
           ?? throw new KeyNotFoundException("Користувача не знайдено.");

    private static BodyParamsDto ToBodyParamsDto(User user) => new()
    {
        Sex = user.Sex,
        BirthDate = user.BirthDate,
        HeightCm = user.HeightCm,
        WeightKg = user.WeightKg,
        ActivityLevel = user.ActivityLevel,
        WeightGoal = user.WeightGoal,
        UpdatedAt = user.BodyParamsUpdatedAt,
    };

    private static void ValidateBodyParams(BodyParamsRequest request, DateOnly today)
    {
        // Майбутня дата перевіряється першою — інакше юзер побачить повідомлення про вік
        if (request.BirthDate > today)
            throw new InvalidOperationException("Дата народження не може бути в майбутньому");

        var age = CalculateAge(request.BirthDate, today);
        if (age is < MinAge or > MaxAge)
            throw new InvalidOperationException($"Вік має бути від {MinAge} до {MaxAge} років");

        if (request.HeightCm is < MinHeightCm or > MaxHeightCm)
            throw new InvalidOperationException($"Зріст має бути від {MinHeightCm} до {MaxHeightCm} см");

        if (request.WeightKg is < MinWeightKg or > MaxWeightKg)
            throw new InvalidOperationException($"Вага має бути від {MinWeightKg} до {MaxWeightKg} кг");
    }

    // Повних років на дату today
    private static int CalculateAge(DateOnly birthDate, DateOnly today)
    {
        var age = today.Year - birthDate.Year;
        if (birthDate > today.AddYears(-age)) age--;
        return age;
    }
}
