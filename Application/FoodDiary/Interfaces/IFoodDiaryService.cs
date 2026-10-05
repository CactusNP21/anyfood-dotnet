using Application.FoodDiary.DTOs;

namespace Application.FoodDiary.Interfaces;

public interface IFoodDiaryService
{
    Task<FoodDiaryDayDto> GetDayAsync(string userId, DateOnly date);
    Task<IReadOnlyList<FoodDiaryDaySummaryDto>> GetRangeAsync(string userId, DateOnly from, DateOnly to);
    Task<FoodDiaryDayDto> ReplaceEntriesAsync(string userId, DateOnly date, ICollection<FoodDiaryEntryRequest> entries);
    Task<FoodDiaryDayDto> SetDayGoalAsync(string userId, DateOnly date, SetDayCalorieGoalRequest request);
    Task<FoodDiaryDayDto> ClearDayGoalAsync(string userId, DateOnly date);
    Task SetDefaultGoalAsync(string userId, SetCalorieGoalRequest request);
}
