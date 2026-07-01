using Microsoft.EntityFrameworkCore;
using SANTA.PoS.Business.Interfaces;
using SANTA.PoS.Data.Persistence;
using SANTA.PoS.Domain.Entities;

namespace SANTA.PoS.Data.Repositories;

public class RequisicionRepository(SantaContext context) : IRequisicionRepository
{
    private readonly SantaContext _context = context;

    public async Task<IEnumerable<RequisicionItem>> GetByUsuarioAsync(string usuclave)
    {
        return await _context.RequisicionItems
            .Include(r => r.Producto)
            .Where(r => r.Usuclave == usuclave)
            .ToListAsync();
    }

    public async Task SaveAsync(string usuclave, List<RequisicionItem> items)
    {
        var existing = _context.RequisicionItems.Where(r => r.Usuclave == usuclave);
        _context.RequisicionItems.RemoveRange(existing);

        foreach (var item in items)
        {
            item.Usuclave = usuclave;
            _context.RequisicionItems.Add(item);
        }

        await _context.SaveChangesAsync();
    }

    public async Task ClearAsync(string usuclave)
    {
        var existing = _context.RequisicionItems.Where(r => r.Usuclave == usuclave);
        _context.RequisicionItems.RemoveRange(existing);
        await _context.SaveChangesAsync();
    }
}
