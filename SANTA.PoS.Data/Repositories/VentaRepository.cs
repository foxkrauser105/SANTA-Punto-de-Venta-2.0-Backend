using Microsoft.EntityFrameworkCore;
using SANTA.PoS.Business.Interfaces;
using SANTA.PoS.Data.Persistence;
using SANTA.PoS.Domain.Entities;
using SANTA.PoS.Domain.Exceptions;
using System.Linq.Expressions;

namespace SANTA.PoS.Data.Repositories;

public class VentaRepository(SantaContext context) : BaseRepository<Venta, int>(context), IVentaRepository
{
    protected override DbSet<Venta> GetDbSet()
    {
        return _context.Venta;
    }

    public override async Task<IEnumerable<Venta>> GetAllAsync()
    {
        return await GetDbSet()
            .Include(v => v.RegistroVentas)
            .ThenInclude(rv => rv.Producto)
            .ToListAsync();
    }

    public override async Task<Venta?> GetByIdAsync(int id)
    {
        return await GetDbSet()
            .Include(v => v.RegistroVentas)
            .ThenInclude(rv => rv.Producto)
            .FirstOrDefaultAsync(v => v.IdVenta == id);
    }

    public async Task<IEnumerable<Venta>> GetFilteredVentasAsync(Expression<Func<Venta, bool>> predicate)
    {
        return await GetDbSet()
            .Include(v => v.RegistroVentas)
            .ThenInclude(rv => rv.Producto)
            .Where(predicate)
            .ToListAsync();
    }

    public async Task<Venta?> GetVentaWithDetallesAsync(int ventaId)
    {
        return await GetDbSet()
            .Include(v => v.RegistroVentas)
            .ThenInclude(rv => rv.Producto)
            .FirstOrDefaultAsync(v => v.IdVenta == ventaId);
    }

    public async Task<Venta> CreateVentaWithDetallesAsync(Venta venta, List<RegistroVenta> registroVentas)
    {
        ArgumentNullException.ThrowIfNull(venta, nameof(venta));
        ArgumentNullException.ThrowIfNull(registroVentas, nameof(registroVentas));

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Create the Venta record
            await GetDbSet().AddAsync(venta);
            await _context.SaveChangesAsync();

            // Attach RegistroVenta records
            foreach (var registroVenta in registroVentas)
            {
                registroVenta.IdVenta = venta.IdVenta;
                await _context.RegistroVentas.AddAsync(registroVenta);
            }
            await _context.SaveChangesAsync();

            // Update product quantities
            foreach (var registroVenta in registroVentas)
            {
                var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Id == registroVenta.ProductoId);

                if (producto is null)
                {
                    throw new DomainException($"Producto con ID '{registroVenta.ProductoId}' no encontrado.");
                }

                // Reduce the product quantity, but not below 0
                producto.Cantidad = Math.Max(0, producto.Cantidad - registroVenta.Cantidad);
                _context.Productos.Update(producto);
            }
            await _context.SaveChangesAsync();

            // Commit the transaction
            await transaction.CommitAsync();

            // Reload the venta with its details
            return await GetVentaWithDetallesAsync(venta.IdVenta) 
                ?? throw new DomainException("Error al crear la venta.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new DomainException($"Error al procesar la venta: {ex.Message}", ex);
        }
    }

    public async Task DeleteLineaVentaAsync(int ventaId, int idRegistro)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var registro = await _context.RegistroVentas
                .Include(r => r.Producto)
                .FirstOrDefaultAsync(r => r.IdRegistro == idRegistro && r.IdVenta == ventaId)
                ?? throw new DomainException($"Línea de venta '{idRegistro}' no encontrada en la venta '{ventaId}'.");

            var venta = await GetByIdAsync(ventaId)
                ?? throw new DomainException($"Venta '{ventaId}' no encontrada.");

            registro.Producto.Cantidad += registro.Cantidad;
            _context.Productos.Update(registro.Producto);

            venta.ImporteVenta -= registro.Precio ?? 0;
            _context.Venta.Update(venta);

            _context.RegistroVentas.Remove(registro);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex) when (ex is not DomainException)
        {
            await transaction.RollbackAsync();
            throw new DomainException($"Error al devolver la línea de venta: {ex.Message}", ex);
        }
    }
}
