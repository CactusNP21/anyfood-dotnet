using anyfood_dotnet.Dtos;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace anyfood_dotnet.Food.Controllers;

public class IngredientSearchRequest
{
    public string? Name { get; set; }
}

[ApiController] // Вказує, що це API контролер
[Route("api/[controller]")] // Встановлює базовий маршрут як "api/food"
public class IngredientController(DataBaseContext db) : ControllerBase
{
    // Конструктор, який отримує DbContext через dependency injection


    // Отримує список всіх страв
    [HttpGet]
    public async Task<ActionResult<IEnumerable<IngredientDto>>> GetFoods()
    {
        return await db.ingredients.ProjectToType<IngredientDto>().ToListAsync();
        
    }
    
    // POST: api/ingredient/search
    // Шукає інгредієнти за назвою
    [HttpPost("search")]
    public async Task<ActionResult<IEnumerable<IngredientDto>>> SearchIngredients([FromBody] IngredientSearchRequest request)
    {
        var query = db.ingredients.AsQueryable();

        // Фільтруємо за назвою, якщо вона вказана
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            query = query.Where(i => i.Name.Contains(request.Name));
        }

        var results = await query
            .ProjectToType<IngredientDto>()
            .ToListAsync();

        return Ok(results);
    }
    
    // GET: api/foods/5
    // Отримує одну страву за її id
    [HttpGet("{id}")]
    public async Task<ActionResult<IngredientDto>> GetFood(int id)
    {
        // ✅ Знову використовуємо .ProjectToType<FoodDto>()
        var foodDto = await db.ingredients
            .Where(f => f.Id == id)
            .ProjectToType<IngredientDto>()
            .FirstOrDefaultAsync();

        if (foodDto == null)
        {
            return NotFound();
        }

        return foodDto;
    }

    // POST: api/foods
    // Створює нову страву
    [HttpPost]
    public async Task<ActionResult<Models.Ingredient>> PostFood(Models.Ingredient ingredient)
    {
        db.ingredients.Add(ingredient);
        await db.SaveChangesAsync();

        // Повертає 201 Created і посилання на новостворений ресурс
        return CreatedAtAction(nameof(GetFood), new { id = ingredient.Id }, ingredient);
    }

    // PUT: api/foods/5
    // Оновлює існуючу страву
    [HttpPut("{id}")]
    public async Task<IActionResult> PutFood(int id, Models.Ingredient ingredient)
    {
        if (id != ingredient.Id)
        {
            return BadRequest(); // Повертає 400 Bad Request, якщо id не збігаються
        }

        db.Entry(ingredient).State = EntityState.Modified;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!db.ingredients.Any(e => e.Id == id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent(); // Повертає 204 No Content після успішного оновлення
    }

    // DELETE: api/foods/5
    // Видаляє страву
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFood(int id)
    {
        var food = await db.ingredients.FindAsync(id);
        if (food == null)
        {
            return NotFound();
        }

        db.ingredients.Remove(food);
        await db.SaveChangesAsync();

        return NoContent(); // Повертає 204 No Content після успішного видалення
    }
}