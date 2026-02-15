namespace anyfood_dotnet.Models;

public class Ingredient
{
    // Id - це унікальний ідентифікатор.
    // База даних автоматично присвоюватиме йому нове значення для кожного запису.
    public int Id { get; set; }

    // Назва страви. `string.Empty` означає, що за замовчуванням це буде порожній рядок.
    public string Name { get; set; } = string.Empty;

    // URL-адреса зображення страви.
    public string ImageUrl { get; set; } = string.Empty;

    // Кількість калорій. `double` - це тип для чисел з десятковою частиною.
    public double Calories { get; set; }

    // Кількість білків у грамах.
    public double Protein { get; set; }

    // Кількість жирів у грамах.
    public double Fat { get; set; }

    // Кількість вуглеводів у грамах.
    public double Carbs { get; set; }
    
    // ✅ ДОДАЙТЕ ЦІ ДВІ ВЛАСТИВОСТІ:
    // 1. Зовнішній ключ до таблиці Categories
    public int CategoryId { get; set; }
    // 2. Навігаційна властивість для доступу до пов'язаного об'єкта Category
    public Category? Category { get; set; }
}