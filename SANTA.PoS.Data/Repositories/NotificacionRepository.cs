using Microsoft.EntityFrameworkCore;
using SANTA.PoS.Business.Interfaces;
using SANTA.PoS.Data.Persistence;
using SANTA.PoS.Domain.Entities;

namespace SANTA.PoS.Data.Repositories;

public class NotificacionRepository(SantaContext context) : BaseRepository<Notificacione, int>(context), INotificacionRepository
{
    protected override DbSet<Notificacione> GetDbSet() => _context.Notificaciones;

    public async Task<IEnumerable<Notificacione>> GetFilteredAsync(
        bool incluirInactivosTerminados,
        string? tipo,
        string? prioridad,
        int? status,
        DateTime? fechaAlta,
        DateTime? fechaTermino,
        int? idMensaje)
    {
        var query = GetDbSet().AsQueryable();

        if (!incluirInactivosTerminados)
            query = query.Where(n => n.Status == 1);

        if (tipo is not null)
            query = query.Where(n => n.Tipo == tipo);

        if (prioridad is not null)
            query = query.Where(n => n.Prioridad == prioridad);

        if (status.HasValue)
            query = query.Where(n => n.Status == status.Value);

        if (fechaAlta.HasValue)
            query = query.Where(n => n.FechaAlta.Date == fechaAlta.Value.Date);

        if (fechaTermino.HasValue)
            query = query.Where(n => n.FechaTermino.HasValue && n.FechaTermino.Value.Date == fechaTermino.Value.Date);

        if (idMensaje.HasValue)
            query = query.Where(n => n.IdMensaje == idMensaje.Value);

        return await query.ToListAsync();
    }
}
