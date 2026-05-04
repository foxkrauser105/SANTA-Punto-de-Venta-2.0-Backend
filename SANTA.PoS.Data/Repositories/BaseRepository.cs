using Microsoft.EntityFrameworkCore;
using SANTA.PoS.Business.Interfaces;
using SANTA.PoS.Data.Persistence;

namespace SANTA.PoS.Data.Repositories;

/// <summary>
/// Generic repository implementation - extend this to have a base for all repositories, reducing code duplication and centralizing common data access logic.
/// </summary>
public abstract class BaseRepository<TClass, TKey>(SantaContext context) : IBaseRepository<TClass, TKey> where TClass : class
{
    protected readonly SantaContext _context = context;

    protected abstract DbSet<TClass> GetDbSet();

    public virtual async Task<TClass> CreateAsync(TClass entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        await GetDbSet().AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<TClass?> GetByIdAsync(TKey id)
    {
        ArgumentNullException.ThrowIfNull(id, nameof(id));
        return await GetDbSet().FindAsync(id);
    }

    public virtual async Task<IEnumerable<TClass>> GetAllAsync()
    {
        return await GetDbSet().ToListAsync();
    }

    public virtual async Task UpdateAsync(TClass entity)
    {
        GetDbSet().Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(TKey id)
    {
        ArgumentNullException.ThrowIfNull(id, nameof(id));
        var entity = await GetByIdAsync(id);
        if (entity is not null)
        {
            GetDbSet().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
