using Domain.Entities;

namespace Application.DayPlans.Interfaces;

public interface IDayPlanRepository
{
    Task<DayPlan?> GetByIdAsync(int id);
    Task<DayPlan?> GetByUserAndDateAsync(string userId, DateTime date);
    Task<IReadOnlyList<DayPlan>> GetByUserAsync(string userId);
    Task<DayPlan> CreateAsync(DayPlan dayPlan);
    Task DeleteAsync(DayPlan dayPlan);
    Task<DayPlan?> GetByIdWithDetailsAsync(int id);
}