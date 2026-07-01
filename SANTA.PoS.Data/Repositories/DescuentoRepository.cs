using Microsoft.EntityFrameworkCore;
using SANTA.PoS.Business.Interfaces;
using SANTA.PoS.Data.Persistence;
using SANTA.PoS.Domain.Entities;

namespace SANTA.PoS.Data.Repositories;

public class DescuentoRepository(SantaContext context) : BaseRepository<Descuento, int>(context), IDescuentoRepository
{
    protected override DbSet<Descuento> GetDbSet() => _context.Descuentos;

    public async Task<IEnumerable<Descuento>> GetAllWithProductoAsync()
    {
        return await _context.Descuentos
            .Include(d => d.Producto)
            .ToListAsync();
    }

    public async Task<Descuento?> GetByIdProductoAsync(string idProducto)
    {
        return await _context.Descuentos
            .Include(d => d.Producto)
            .FirstOrDefaultAsync(d => d.Producto.IdProducto == idProducto);
    }

    public async Task DeleteByIdProductoAsync(string idProducto)
    {
        var descuento = await GetByIdProductoAsync(idProducto);
        if (descuento is not null)
        {
            _context.Descuentos.Remove(descuento);
            await _context.SaveChangesAsync();
        }
    }
}
