using Domain.Entities;

namespace Application.FoodDiary.Interfaces;

public interface IFoodDiaryRepository
{
    Task<FoodDiaryDay?> GetDayAsync(string userId, DateOnly date);
    Task<IReadOnlyList<FoodDiaryDay>> GetDaysInRangeAsync(string userId, DateOnly from, DateOnly to);
    // Add/Remove лише відстежують зміни — зберігає SaveChangesAsync
    void AddDay(FoodDiaryDay day);
    void RemoveDay(FoodDiaryDay day);
    void AddGoal(CalorieGoal goal);
    Task SaveChangesAsync();

    // Остання ціль з EffectiveFrom <= date
    Task<CalorieGoal?> GetGoalForDateAsync(string userId, DateOnly date);
    // Ціль, що діяла на початок діапазону, плюс усі зміни всередині нього (за зростанням дати)
    Task<IReadOnlyList<CalorieGoal>> GetGoalsForRangeAsync(string userId, DateOnly from, DateOnly to);
    Task<CalorieGoal?> GetGoalByEffectiveFromAsync(string userId, DateOnly effectiveFrom);

    Task<IReadOnlyList<int>> GetMissingProductIdsAsync(IReadOnlyCollection<int> ids);
    Task<IReadOnlyList<int>> GetMissingRecipeIdsAsync(IReadOnlyCollection<int> ids);
}
