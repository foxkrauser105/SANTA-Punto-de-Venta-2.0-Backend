using Microsoft.EntityFrameworkCore;
using SANTA.PoS.Business.Interfaces;
using SANTA.PoS.Data.Persistence;
using SANTA.PoS.Domain.Entities;
using System.Linq.Expressions;

namespace SANTA.PoS.Data.Repositories
{
    public class ProductRepository(SantaContext context) : BaseRepository<Producto, string>(context), IProductRepository
    {
        protected override DbSet<Producto> GetDbSet()
        {
            return _context.Productos;
        }

        public Task<IEnumerable<Producto>> GetFilteredProductsAsync(Expression<Func<Producto, bool>> predicate)
        {
            return Task.FromResult(_context.Productos.Where(predicate).AsEnumerable());
        }
    }
}
