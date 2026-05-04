namespace SANTA.PoS.Business.Interfaces;

public interface IBaseRepository<TClass, TKey> where TClass : class
{
    Task<IEnumerable<TClass>> GetAllAsync();
    Task<TClass?> GetByIdAsync(TKey id);
    Task<TClass> CreateAsync(TClass entity);
    Task UpdateAsync(TClass entity);
    Task DeleteAsync(TKey id);
}
