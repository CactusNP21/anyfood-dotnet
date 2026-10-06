using Application.FoodDiary.DTOs;
using Application.FoodDiary.Interfaces;
using Domain.Entities;

namespace Application.FoodDiary.Services;

public class FoodDiaryService(IFoodDiaryRepository repository) : IFoodDiaryService
{
    private const int MaxRangeDays = 366;

    public async Task<FoodDiaryDayDto> GetDayAsync(string userId, DateOnly date)
    {
        var day = await repository.GetDayAsync(userId, date);
        var goal = await repository.GetGoalForDateAsync(userId, date);
        return ToDayDto(date, day, goal);
    }

    public async Task<IReadOnlyList<FoodDiaryDaySummaryDto>> GetRangeAsync(string userId, DateOnly from, DateOnly to)
    {
        if (from > to)
            throw new InvalidOperationException("Дата початку не може бути пізніше дати кінця.");

        if (to.DayNumber - from.DayNumber + 1 > MaxRangeDays)
            throw new InvalidOperationException($"Діапазон не може перевищувати {MaxRangeDays} днів.");

        var days = (await repository.GetDaysInRangeAsync(userId, from, to)).ToDictionary(d => d.Date);
        var goals = await repository.GetGoalsForRangeAsync(userId, from, to);

        var result = new List<FoodDiaryDaySummaryDto>();
        var goalIndex = -1;

        for (var date = from; date <= to; date = date.AddDays(1))
        {
            // goals відсортовані за EffectiveFrom — просуваємось, поки наступна ціль вже діє
            while (goalIndex + 1 < goals.Count && goals[goalIndex + 1].EffectiveFrom <= date)
                goalIndex++;

            var day = days.GetValueOrDefault(date);
            var dto = ToDayDto(date, day, goalIndex >= 0 ? goals[goalIndex] : null);

            result.Add(new FoodDiaryDaySummaryDto
            {
                Date = dto.Date,
                CalorieGoal = dto.CalorieGoal,
                IsGoalOverridden = dto.IsGoalOverridden,
                TotalCalories = dto.TotalCalories,
                TotalProtein = dto.TotalProtein,
                TotalFat = dto.TotalFat,
                TotalCarbs = dto.TotalCarbs,
                TotalSalt = dto.TotalSalt,
            });
        }

        return result;
    }

    public async Task<FoodDiaryDayDto> ReplaceEntriesAsync(
        string userId, DateOnly date, ICollection<FoodDiaryEntryRequest> entries)
    {
        await ValidateEntriesAsync(entries);

        var day = await repository.GetDayAsync(userId, date);

        if (day is null)
        {
            if (entries.Count == 0)
                return await GetDayAsync(userId, date);

            day = new FoodDiaryDay { UserId = userId, Date = date };
            repository.AddDay(day);
        }

        day.Entries.Clear();
        foreach (var e in entries)
        {
            day.Entries.Add(new FoodDiaryEntry
            {
                ProductId = e.ProductId,
                RecipeId = e.RecipeId,
                Weight = e.Weight,
                Time = e.Time,
            });
        }

        await SaveOrRemoveEmptyDayAsync(day);
        return await GetDayAsync(userId, date);
    }

    public async Task<FoodDiaryDayDto> SetDayGoalAsync(string userId, DateOnly date, SetDayCalorieGoalRequest request)
    {
        ValidateCalories(request.Calories);

        var day = await repository.GetDayAsync(userId, date);
        if (day is null)
        {
            day = new FoodDiaryDay { UserId = userId, Date = date };
            repository.AddDay(day);
        }

        day.CalorieGoalOverride = request.Calories;
        await repository.SaveChangesAsync();

        return await GetDayAsync(userId, date);
    }

    public async Task<FoodDiaryDayDto> ClearDayGoalAsync(string userId, DateOnly date)
    {
        var day = await repository.GetDayAsync(userId, date);
        if (day is not null)
        {
            day.CalorieGoalOverride = null;
            await SaveOrRemoveEmptyDayAsync(day);
        }

        return await GetDayAsync(userId, date);
    }

    public async Task SetDefaultGoalAsync(string userId, SetCalorieGoalRequest request)
    {
        ValidateCalories(request.Calories);

        // Повторна зміна в той самий день — оновлюємо запис, а не додаємо дубль
        var existing = await repository.GetGoalByEffectiveFromAsync(userId, request.EffectiveFrom);
        if (existing is not null)
        {
            existing.Calories = request.Calories;
            await repository.SaveChangesAsync();
            return;
        }

        repository.AddGoal(new CalorieGoal
        {
            UserId = userId,
            EffectiveFrom = request.EffectiveFrom,
            Calories = request.Calories,
        });
        await repository.SaveChangesAsync();
    }

    // ── Private ──────────────────────────────────────────────────────────────

    // День без записів і без ручної цілі нічого не зберігає — видаляємо рядок
    private async Task SaveOrRemoveEmptyDayAsync(FoodDiaryDay day)
    {
        if (day.Entries.Count == 0 && day.CalorieGoalOverride is null)
            repository.RemoveDay(day);

        await repository.SaveChangesAsync();
    }

    private static FoodDiaryDayDto ToDayDto(DateOnly date, FoodDiaryDay? day, CalorieGoal? goal)
    {
        var entries = (day?.Entries ?? [])
            .OrderBy(e => e.Time)
            .Select(ToEntryDto)
            .ToList();

        return new FoodDiaryDayDto
        {
            Date = date,
            CalorieGoal = day?.CalorieGoalOverride ?? goal?.Calories,
            IsGoalOverridden = day?.CalorieGoalOverride is not null,
            TotalCalories = entries.Sum(e => e.Calories),
            TotalProtein = entries.Sum(e => e.Protein),
            TotalFat = entries.Sum(e => e.Fat),
            TotalCarbs = entries.Sum(e => e.Carbs),
            TotalSalt = entries.Sum(e => e.Salt),
            Entries = entries,
        };
    }

    private static FoodDiaryEntryDto ToEntryDto(FoodDiaryEntry entry)
    {
        var ratio = entry.Weight / 100f;
        var dto = new FoodDiaryEntryDto
        {
            Id = entry.Id,
            ProductId = entry.ProductId,
            RecipeId = entry.RecipeId,
            Name = entry.Product?.Name ?? entry.Recipe?.Name ?? string.Empty,
            ImageUrl = entry.Product?.ImageUrl ?? entry.Recipe?.ImageUrl,
            Weight = entry.Weight,
            Time = entry.Time,
        };

        if (entry.Product is not null)
        {
            dto.Calories = (float)entry.Product.Calories * ratio;
            dto.Protein  = (float)entry.Product.Protein  * ratio;
            dto.Fat      = (float)entry.Product.Fat      * ratio;
            dto.Carbs    = (float)entry.Product.Carbs    * ratio;
            dto.Salt     = (float)entry.Product.Salt     * ratio;
        }
        else if (entry.Recipe is not null)
        {
            dto.Calories = entry.Recipe.Calories * ratio;
            dto.Protein  = entry.Recipe.Protein  * ratio;
            dto.Fat      = entry.Recipe.Fat      * ratio;
            dto.Carbs    = entry.Recipe.Carbs    * ratio;
            dto.Salt     = entry.Recipe.Salt     * ratio;
        }

        return dto;
    }

    private async Task ValidateEntriesAsync(ICollection<FoodDiaryEntryRequest> entries)
    {
        foreach (var entry in entries)
        {
            if (entry.RecipeId is null && entry.ProductId is null)
                throw new InvalidOperationException("Кожен запис повинен містити рецепт або продукт.");

            if (entry.RecipeId is not null && entry.ProductId is not null)
                throw new InvalidOperationException("Запис не може одночасно містити рецепт і продукт.");

            if (entry.Weight <= 0)
                throw new InvalidOperationException("Вага повинна бути більше 0.");
        }

        var productIds = entries.Where(e => e.ProductId is not null).Select(e => e.ProductId!.Value).Distinct().ToList();
        var missingProducts = await repository.GetMissingProductIdsAsync(productIds);
        if (missingProducts.Count > 0)
            throw new KeyNotFoundException($"Продукти не знайдено: {string.Join(", ", missingProducts)}.");

        var recipeIds = entries.Where(e => e.RecipeId is not null).Select(e => e.RecipeId!.Value).Distinct().ToList();
        var missingRecipes = await repository.GetMissingRecipeIdsAsync(recipeIds);
        if (missingRecipes.Count > 0)
            throw new KeyNotFoundException($"Рецепти не знайдено: {string.Join(", ", missingRecipes)}.");
    }

    private static void ValidateCalories(int calories)
    {
        if (calories <= 0)
            throw new InvalidOperationException("Ціль калорій повинна бути більше 0.");
    }
}
