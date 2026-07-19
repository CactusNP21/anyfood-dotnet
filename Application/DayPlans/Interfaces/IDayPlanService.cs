using Application.DayPlans.DTOs;

namespace Application.DayPlans.Interfaces;

public interface IDayPlanService
{
    Task<DayPlanDto> CreateAsync(CreateDayPlanRequest request, string userId);
    Task<DayPlanDto> GetByIdAsync(int id);
    Task<IReadOnlyList<UserDayPlanShort>> GetByUserAsync(string userId);
    Task DeleteAsync(int id);
}