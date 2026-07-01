using SANTA.PoS.Domain.Entities;
using System.Linq.Expressions;

namespace SANTA.PoS.Business.Interfaces;

public interface IProductRepository : IBaseRepository<Producto, string>
{
    Task<IEnumerable<Producto>> GetFilteredProductsAsync(Expression<Func<Producto, bool>> predicate);
    Task<bool> ExistsByIdProductoAsync(string idProducto);
    Task UpdateIdProductoAsync(string oldIdProducto, string newIdProducto);
}
