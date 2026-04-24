using Application.Base.Interfaces;
using Application.Categories.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Base;

public class BaseController<TService, TDto, TCreateRequest, TUpdateRequest>(TService baseService): ControllerBase
    where TService: IBaseService<TDto, TCreateRequest, TUpdateRequest>
where TDto : class
where TCreateRequest: class
where TUpdateRequest: class
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TDto>>> GetAll()
    {
        var categories = await baseService.GetAllAsync();
        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TDto>> GetById(int id)
    {
        var category = await baseService.GetByIdAsync(id);
        return Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult<TDto>> Create(TCreateRequest request)
    {
        var category = await baseService.CreateAsync(request);
        return Ok(category);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TDto>> Update(int id, TUpdateRequest request)
    {
        var category = await baseService.UpdateAsync(id, request);
        return Ok(category);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await baseService.DeleteAsync(id);
        return NoContent();
    }
}