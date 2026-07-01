using SANTA.PoS.Domain.Entities;

namespace SANTA.PoS.Business.Interfaces;

public interface INotaCreditoRepository
{
    Task<IEnumerable<NotasCredito>> GetByClienteAsync(int numcliente);
    Task<NotasCredito?> GetByFolioAsync(int numcliente, int ncfolio);
    Task<NotasCredito?> GetOpenByClienteAsync(int numcliente);
    Task<int> GetNextFolioAsync(int numcliente);
    Task<IEnumerable<PagosNotasCredito>> GetPagosAsync(int numcliente, int ncfolio);
    Task<NotasCredito> CreateWithItemsAsync(NotasCredito nota, List<RegistroNotasCredito> items);
    Task AddItemsAsync(int numcliente, int ncfolio, List<RegistroNotasCredito> items);
    Task<PagosNotasCredito> RegistrarPagoAsync(int numcliente, int ncfolio, decimal montoPagado, Venta ventaPago, RegistroVenta registroVentaPago);
    Task CancelarAsync(int numcliente, int ncfolio);
}
