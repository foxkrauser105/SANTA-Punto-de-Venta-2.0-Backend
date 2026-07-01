using Microsoft.EntityFrameworkCore;
using SANTA.PoS.Business.Interfaces;
using SANTA.PoS.Data.Persistence;
using SANTA.PoS.Domain.Entities;

namespace SANTA.PoS.Data.Repositories;

public class UsuarioRepository(SantaContext context) : BaseRepository<Usuario, string>(context), IUsuarioRepository
{
    protected override DbSet<Usuario> GetDbSet() => _context.Usuarios;

    public override async Task<Usuario?> GetByIdAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id, nameof(id));
        return await GetDbSet().FirstOrDefaultAsync(u => u.Usuclave == id);
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
