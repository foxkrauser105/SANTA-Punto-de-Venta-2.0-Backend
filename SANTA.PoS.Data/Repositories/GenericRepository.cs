using SANTA.PoS.Domain.Entities;

namespace SANTA.PoS.Data.Repositories;

/// <summary>
/// Generic repository implementation - extend this for specific entities
/// </summary>
public abstract class GenericRepository<T> : IRepository<T> where T : BaseEntity
{
    // TODO: Implement with DbContext or your preferred ORM
    // This is a template - replace with actual implementation

    public virtual Task<T?> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public virtual Task<IEnumerable<T>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public virtual Task AddAsync(T entity)
    {
        throw new NotImplementedException();
    }

    public virtual Task UpdateAsync(T entity)
    {
        throw new NotImplementedException();
    }

    public virtual Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }

    public virtual Task SaveChangesAsync()
    {
        throw new NotImplementedException();
    }
}
