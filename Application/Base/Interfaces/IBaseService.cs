namespace Application.Base.Interfaces;

public interface IBaseService<TDto, TCreateRequest, TUpdateRequest>
    where TDto : class
where TCreateRequest : class
where TUpdateRequest : class
{
    Task<IReadOnlyList<TDto>> GetAllAsync();
    Task<TDto> GetByIdAsync(int id);
    Task<TDto> CreateAsync(TCreateRequest request);
    Task<TDto> UpdateAsync(int id, TUpdateRequest request);
    Task DeleteAsync(int id);
}