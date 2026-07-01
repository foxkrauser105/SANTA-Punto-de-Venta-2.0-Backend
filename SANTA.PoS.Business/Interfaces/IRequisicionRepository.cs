using SANTA.PoS.Domain.Entities;

namespace SANTA.PoS.Business.Interfaces;

public interface IRequisicionRepository
{
    Task<IEnumerable<RequisicionItem>> GetByUsuarioAsync(string usuclave);
    Task SaveAsync(string usuclave, List<RequisicionItem> items);
    Task ClearAsync(string usuclave);
}
