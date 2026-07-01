using Microsoft.EntityFrameworkCore;
using SANTA.PoS.Business.Interfaces;
using SANTA.PoS.Data.Persistence;
using SANTA.PoS.Domain.Entities;

namespace SANTA.PoS.Data.Repositories;

public class VentaDiaRepository(SantaContext context) : BaseRepository<VentaDia, int>(context), IVentaDiaRepository
{
    protected override DbSet<VentaDia> GetDbSet() => _context.VentaDia;

    public async Task<VentaDia?> GetByFechaAsync(DateOnly fecha)
    {
        return await GetDbSet().FirstOrDefaultAsync(v => v.Fecha == fecha);
    }

    public async Task<decimal> GetVentaPosAsync(DateOnly fecha)
    {
        return await _context.Venta
            .Where(v => v.Fecha == fecha)
            .SumAsync(v => v.ImporteVenta);
    }
}
