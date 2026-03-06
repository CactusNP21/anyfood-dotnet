using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace anyfood_dotnet.Base;

[ApiController]
[Route("api/[controller]")]
public abstract class ControllerRestBase<TEntity, TResponse, TCreateRequest>(AppDbContext context)
    : ControllerBase
    where TEntity : class
    where TResponse : class
    where TCreateRequest : class
{
    [HttpGet]
    public virtual async Task<ActionResult<List<TResponse>>> GetAll()
    {
        var entities = await context.Set<TEntity>().ToListAsync();
        return Ok(entities.Adapt<List<TResponse>>());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TResponse>> GetById(int id)
    {
        var entity = await context.Set<TEntity>().FindAsync(id);
        if (entity is null) return NotFound();

        return Ok(entity.Adapt<TResponse>());
    }

    [HttpPost]
    public virtual async Task<ActionResult<TResponse>> Create(TCreateRequest request)
    {
        var entity = request.Adapt<TEntity>();
        context.Set<TEntity>().Add(entity);
        await context.SaveChangesAsync();

        return Ok(entity.Adapt<TResponse>());
    }
}