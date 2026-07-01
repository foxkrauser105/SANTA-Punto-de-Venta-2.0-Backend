using SANTA.PoS.Domain.Entities;
using System.Linq.Expressions;

namespace SANTA.PoS.Business.Interfaces;

public interface IVentaRepository : IBaseRepository<Venta, int>
{
    Task<IEnumerable<Venta>> GetFilteredVentasAsync(Expression<Func<Venta, bool>> predicate);

    Task<Venta?> GetVentaWithDetallesAsync(int ventaId);

    /// <summary>
    /// Creates a Venta with its RegistroVenta records and updates product quantities in a transaction
    /// </summary>
    Task<Venta> CreateVentaWithDetallesAsync(Venta venta, List<RegistroVenta> registroVentas);

    /// <summary>
    /// Returns a single sale line: restores product quantity, adjusts venta total, removes the registro row
    /// </summary>
    Task DeleteLineaVentaAsync(int ventaId, int idRegistro);
}
