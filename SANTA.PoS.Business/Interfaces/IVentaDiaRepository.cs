using SANTA.PoS.Domain.Entities;

namespace SANTA.PoS.Business.Interfaces;

public interface IVentaDiaRepository : IBaseRepository<VentaDia, int>
{
    Task<VentaDia?> GetByFechaAsync(DateOnly fecha);
    Task<decimal> GetVentaPosAsync(DateOnly fecha);
}
