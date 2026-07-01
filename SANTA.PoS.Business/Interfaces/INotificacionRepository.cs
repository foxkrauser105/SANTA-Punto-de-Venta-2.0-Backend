using SANTA.PoS.Domain.Entities;

namespace SANTA.PoS.Business.Interfaces;

public interface INotificacionRepository : IBaseRepository<Notificacione, int>
{
    Task<IEnumerable<Notificacione>> GetFilteredAsync(
        bool incluirInactivosTerminados,
        string? tipo,
        string? prioridad,
        int? status,
        DateTime? fechaAlta,
        DateTime? fechaTermino,
        int? idMensaje);
}
