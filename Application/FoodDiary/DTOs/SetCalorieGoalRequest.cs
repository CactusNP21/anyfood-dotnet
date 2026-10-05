namespace Application.FoodDiary.DTOs;

// Нова ціль за замовчуванням, діє з EffectiveFrom (локальна дата клієнта) до наступної зміни
public class SetCalorieGoalRequest
{
    public int Calories { get; set; }
    public DateOnly EffectiveFrom { get; set; }
}

// Ціль лише для одного дня
public class SetDayCalorieGoalRequest
{
    public int Calories { get; set; }
}
