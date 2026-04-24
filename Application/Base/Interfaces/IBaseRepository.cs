namespace Application.Base.Interfaces;

public interface IBaseRepository<TDomainEntity>
where TDomainEntity : class
{
    Task<IReadOnlyList<TDomainEntity>> GetAllAsync();
    Task<TDomainEntity?> GetByIdAsync(int id);
    Task<TDomainEntity> CreateAsync(TDomainEntity category);
    Task UpdateAsync(TDomainEntity category);
    Task DeleteAsync(TDomainEntity category);

}