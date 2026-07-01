using SANTA.PoS.Domain.Entities;

namespace SANTA.PoS.Business.Interfaces;

public interface IDescuentoRepository : IBaseRepository<Descuento, int>
{
    Task<IEnumerable<Descuento>> GetAllWithProductoAsync();
    Task<Descuento?> GetByIdProductoAsync(string idProducto);
    Task DeleteByIdProductoAsync(string idProducto);
}
