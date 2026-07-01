using SANTA.PoS.Domain.Entities;

namespace SANTA.PoS.Business.Interfaces;

public interface IDescuentoRepository
{
    Task<IEnumerable<Descuento>> GetAllWithProductoAsync();
    Task<Descuento?> GetByIdProductoAsync(string idProducto);
    Task<Descuento> CreateAsync(Descuento descuento);
    Task UpdateAsync(Descuento descuento);
    Task DeleteByIdProductoAsync(string idProducto);
}
