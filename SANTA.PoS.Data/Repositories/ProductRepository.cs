using Microsoft.EntityFrameworkCore;
using SANTA.PoS.Business.Interfaces;
using SANTA.PoS.Data.Persistence;
using SANTA.PoS.Domain.Entities;
using System.Linq.Expressions;

namespace SANTA.PoS.Data.Repositories;

public class ProductRepository(SantaContext context) : BaseRepository<Producto, string>(context), IProductRepository
{
    protected override DbSet<Producto> GetDbSet()
    {
        return _context.Productos;
    }

    public override async Task<Producto?> GetByIdAsync(string id)
    {
        ArgumentNullException.ThrowIfNull(id, nameof(id));
        return await GetDbSet().FirstOrDefaultAsync(p => p.IdProducto == id);
    }

    public async Task<IEnumerable<Producto>> GetFilteredProductsAsync(Expression<Func<Producto, bool>> predicate)
    {
        return await GetDbSet().Include(p => p.Descuento).Where(predicate).ToListAsync();
    }

    public async Task<bool> ExistsByIdProductoAsync(string idProducto)
    {
        return await GetDbSet().AnyAsync(p => p.IdProducto == idProducto);
    }

    public async Task UpdateIdProductoAsync(string oldIdProducto, string newIdProducto)
    {
        var producto = await GetByIdAsync(oldIdProducto)
            ?? throw new InvalidOperationException($"Producto '{oldIdProducto}' no encontrado.");
        producto.IdProducto = newIdProducto;
        _context.Productos.Update(producto);
        await _context.SaveChangesAsync();
    }
}