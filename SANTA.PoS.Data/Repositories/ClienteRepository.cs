using Microsoft.EntityFrameworkCore;
using SANTA.PoS.Business.Interfaces;
using SANTA.PoS.Data.Persistence;
using SANTA.PoS.Domain.Entities;

namespace SANTA.PoS.Data.Repositories;

public class ClienteRepository(SantaContext context) : BaseRepository<Cliente, int>(context), IClienteRepository
{
    protected override DbSet<Cliente> GetDbSet() => _context.Clientes;

    public async Task<int> GetNextFolioAsync()
    {
        var max = await GetDbSet().MaxAsync(c => (int?)c.Numcliente) ?? 0;
        return max + 1;
    }
}
