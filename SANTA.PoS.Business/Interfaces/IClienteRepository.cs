using SANTA.PoS.Domain.Entities;

namespace SANTA.PoS.Business.Interfaces;

public interface IClienteRepository : IBaseRepository<Cliente, int>
{
    Task<int> GetNextFolioAsync();
}
