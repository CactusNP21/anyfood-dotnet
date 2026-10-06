using System.Globalization;
using Application.Auth.Interfaces;
using Application.Users.DTOs;
using Application.Users.Services;
using Domain.Entities;
using Domain.Enums;

namespace Application.Tests.Users;

public class UserServiceBodyParamsTests
{
    private const string UserId = "user-1";
    private static readonly DateTimeOffset Now = new(2026, 10, 6, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = DateOnly.FromDateTime(Now.UtcDateTime);

    private readonly FakeUserRepository repository = new();
    private readonly UserService service;

    public UserServiceBodyParamsTests()
    {
        repository.Users.Add(new User { Id = UserId, Name = "Test", UserName = "tester", Email = "t@t.com" });
        service = new UserService(repository, new FixedTimeProvider(Now));
    }

    [Fact]
    public async Task GetProfile_UserWithoutBodyParams_ReturnsBodyParamsWithAllFieldsNull()
    {
        var profile = await service.GetProfileAsync(UserId);

        Assert.NotNull(profile.BodyParams);
        Assert.Null(profile.BodyParams.Sex);
        Assert.Null(profile.BodyParams.BirthDate);
        Assert.Null(profile.BodyParams.HeightCm);
        Assert.Null(profile.BodyParams.WeightKg);
        Assert.Null(profile.BodyParams.ActivityLevel);
        Assert.Null(profile.BodyParams.WeightGoal);
        Assert.Null(profile.BodyParams.UpdatedAt);
    }

    [Fact]
    public async Task UpdateBodyParams_ValidRequest_SavesRoundedValuesAndSetsUpdatedAt()
    {
        var result = await service.UpdateBodyParamsAsync(UserId, ValidRequest() with
        {
            HeightCm = 180.24m,
            WeightKg = 72.46m,
        });

        Assert.Equal(180.2m, result.HeightCm);
        Assert.Equal(72.5m, result.WeightKg);
        Assert.Equal(Sex.Female, result.Sex);
        Assert.Equal(ActivityLevel.VeryActive, result.ActivityLevel);
        Assert.Equal(WeightGoal.Lose, result.WeightGoal);
        Assert.Equal(Now, result.UpdatedAt);
        Assert.Equal(1, repository.UpdateCount);

        var profile = await service.GetProfileAsync(UserId);
        Assert.Equal(72.5m, profile.BodyParams.WeightKg);
        Assert.Equal(Now, profile.BodyParams.UpdatedAt);
    }

    [Fact]
    public async Task UpdateBodyParams_BirthDateInFuture_ReturnsFutureMessage()
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateBodyParamsAsync(UserId, ValidRequest() with { BirthDate = Today.AddDays(1) }));

        Assert.Equal("Дата народження не може бути в майбутньому", ex.Message);
        Assert.Equal(0, repository.UpdateCount);
    }

    // birthDate = today - years + dayOffset
    [Theory]
    [InlineData(15, 0)]   // рівно 15 сьогодні
    [InlineData(100, 0)]  // рівно 100 сьогодні
    [InlineData(101, 1)]  // 101-й день народження завтра — ще 100
    public async Task UpdateBodyParams_AgeWithinRange_Succeeds(int years, int dayOffset)
    {
        var birthDate = Today.AddYears(-years).AddDays(dayOffset);

        var result = await service.UpdateBodyParamsAsync(UserId, ValidRequest() with { BirthDate = birthDate });

        Assert.Equal(birthDate, result.BirthDate);
    }

    [Theory]
    [InlineData(15, 1)]   // 15-й день народження завтра — ще 14
    [InlineData(101, 0)]  // рівно 101
    [InlineData(0, 0)]    // народився сьогодні
    public async Task UpdateBodyParams_AgeOutOfRange_ReturnsAgeMessage(int years, int dayOffset)
    {
        var birthDate = Today.AddYears(-years).AddDays(dayOffset);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateBodyParamsAsync(UserId, ValidRequest() with { BirthDate = birthDate }));

        Assert.Equal("Вік має бути від 15 до 100 років", ex.Message);
        Assert.Equal(0, repository.UpdateCount);
    }

    [Theory]
    [InlineData("100")]
    [InlineData("250")]
    public async Task UpdateBodyParams_HeightOnBoundary_Succeeds(string height)
    {
        var result = await service.UpdateBodyParamsAsync(UserId, ValidRequest() with { HeightCm = Dec(height) });

        Assert.Equal(Dec(height), result.HeightCm);
    }

    [Theory]
    [InlineData("99.9")]
    [InlineData("250.1")]
    public async Task UpdateBodyParams_HeightOutOfRange_ReturnsHeightMessage(string height)
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateBodyParamsAsync(UserId, ValidRequest() with { HeightCm = Dec(height) }));

        Assert.Equal("Зріст має бути від 100 до 250 см", ex.Message);
        Assert.Equal(0, repository.UpdateCount);
    }

    [Theory]
    [InlineData("30")]
    [InlineData("300")]
    public async Task UpdateBodyParams_WeightOnBoundary_Succeeds(string weight)
    {
        var result = await service.UpdateBodyParamsAsync(UserId, ValidRequest() with { WeightKg = Dec(weight) });

        Assert.Equal(Dec(weight), result.WeightKg);
    }

    [Theory]
    [InlineData("29.9")]
    [InlineData("300.1")]
    public async Task UpdateBodyParams_WeightOutOfRange_ReturnsWeightMessage(string weight)
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateBodyParamsAsync(UserId, ValidRequest() with { WeightKg = Dec(weight) }));

        Assert.Equal("Вага має бути від 30 до 300 кг", ex.Message);
        Assert.Equal(0, repository.UpdateCount);
    }

    [Fact]
    public async Task ClearBodyParams_ResetsAllFieldsToNull()
    {
        await service.UpdateBodyParamsAsync(UserId, ValidRequest());

        await service.ClearBodyParamsAsync(UserId);

        var bodyParams = (await service.GetProfileAsync(UserId)).BodyParams;
        Assert.Null(bodyParams.Sex);
        Assert.Null(bodyParams.BirthDate);
        Assert.Null(bodyParams.HeightCm);
        Assert.Null(bodyParams.WeightKg);
        Assert.Null(bodyParams.ActivityLevel);
        Assert.Null(bodyParams.WeightGoal);
        Assert.Null(bodyParams.UpdatedAt);
    }

    [Fact]
    public async Task UpdateBodyParams_UnknownUser_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateBodyParamsAsync("missing", ValidRequest()));
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    // decimal не можна передати в [InlineData] — передаємо рядком
    private static decimal Dec(string value) => decimal.Parse(value, CultureInfo.InvariantCulture);

    private static TestBodyParamsRequest ValidRequest() => new()
    {
        Sex = Sex.Female,
        BirthDate = new DateOnly(1995, 5, 20),
        HeightCm = 170,
        WeightKg = 65,
        ActivityLevel = ActivityLevel.VeryActive,
        WeightGoal = WeightGoal.Lose,
    };

    // record-обгортка, щоб змінювати одне поле через `with`
    private record TestBodyParamsRequest
    {
        public Sex Sex { get; init; }
        public DateOnly BirthDate { get; init; }
        public decimal HeightCm { get; init; }
        public decimal WeightKg { get; init; }
        public ActivityLevel ActivityLevel { get; init; }
        public WeightGoal WeightGoal { get; init; }

        public static implicit operator BodyParamsRequest(TestBodyParamsRequest r) => new()
        {
            Sex = r.Sex,
            BirthDate = r.BirthDate,
            HeightCm = r.HeightCm,
            WeightKg = r.WeightKg,
            ActivityLevel = r.ActivityLevel,
            WeightGoal = r.WeightGoal,
        };
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public List<User> Users { get; } = [];
        public int UpdateCount { get; private set; }

        public Task<User?> FindByIdAsync(string id) => Task.FromResult(Users.FirstOrDefault(u => u.Id == id));

        public Task UpdateAsync(User user)
        {
            UpdateCount++;
            return Task.CompletedTask;
        }

        public Task CreateExternalAsync(User user) => throw new NotSupportedException();
        public Task<IList<string>> GetRolesAsync(User user) => throw new NotSupportedException();
        public Task<User?> FindByUsernameAsync(string username) => throw new NotSupportedException();
        public Task<User?> FindByEmailAsync(string email) => throw new NotSupportedException();
        public Task<User?> FindByRefreshTokenAsync(string refreshToken) => throw new NotSupportedException();
        public Task<bool> CheckPasswordAsync(User user, string password) => throw new NotSupportedException();
        public Task CreateAsync(User user, string password) => throw new NotSupportedException();
    }
}
