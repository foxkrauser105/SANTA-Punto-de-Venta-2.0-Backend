using Microsoft.EntityFrameworkCore;
using SANTA.PoS.Business.Interfaces;
using SANTA.PoS.Data.Persistence;
using SANTA.PoS.Domain.Entities;

namespace SANTA.PoS.Data.Repositories;

public class CategoriaRepository(SantaContext context) : BaseRepository<Categoria, string>(context), ICategoriaRepository
{
    protected override DbSet<Categoria> GetDbSet() => _context.Categorias;

    public override async Task<Categoria?> GetByIdAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id, nameof(id));
        return await GetDbSet().FirstOrDefaultAsync(c => c.Nombre == id);
    }

    public override async Task DeleteAsync(string id)
    {
        var entity = await GetByIdAsync(id);
        if (entity is not null)
        {
            GetDbSet().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
