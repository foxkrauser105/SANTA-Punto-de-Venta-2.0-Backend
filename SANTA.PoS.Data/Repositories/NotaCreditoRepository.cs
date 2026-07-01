using Microsoft.EntityFrameworkCore;
using SANTA.PoS.Business.Interfaces;
using SANTA.PoS.Data.Persistence;
using SANTA.PoS.Domain.Entities;
using SANTA.PoS.Domain.Exceptions;

namespace SANTA.PoS.Data.Repositories;

public class NotaCreditoRepository(SantaContext context) : INotaCreditoRepository
{
    private readonly SantaContext _context = context;

    public async Task<IEnumerable<NotasCredito>> GetByClienteAsync(int numcliente)
    {
        return await _context.NotasCreditos
            .Where(n => n.Numcliente == numcliente)
            .ToListAsync();
    }

    public async Task<NotasCredito?> GetByFolioAsync(int numcliente, int ncfolio)
    {
        return await _context.NotasCreditos
            .FirstOrDefaultAsync(n => n.Numcliente == numcliente && n.Ncfolio == ncfolio);
    }

    public async Task<NotasCredito?> GetOpenByClienteAsync(int numcliente)
    {
        return await _context.NotasCreditos
            .FirstOrDefaultAsync(n => n.Numcliente == numcliente
                && n.Status != "CO" && n.Status != "CA");
    }

    public async Task<int> GetNextFolioAsync(int numcliente)
    {
        var max = await _context.NotasCreditos
            .Where(n => n.Numcliente == numcliente)
            .MaxAsync(n => (int?)n.Ncfolio) ?? 0;
        return max + 1;
    }

    public async Task<IEnumerable<PagosNotasCredito>> GetPagosAsync(int numcliente, int ncfolio)
    {
        return await _context.PagosNotasCreditos
            .Where(p => p.Numcliente == numcliente && p.Ncfolio == ncfolio)
            .ToListAsync();
    }

    public async Task<NotasCredito> CreateWithItemsAsync(NotasCredito nota, List<RegistroNotasCredito> items)
    {
        ArgumentNullException.ThrowIfNull(nota);
        ArgumentNullException.ThrowIfNull(items);

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.NotasCreditos.Add(nota);
            await _context.SaveChangesAsync();

            foreach (var item in items)
            {
                item.Numcliente = nota.Numcliente;
                item.Ncfolio = nota.Ncfolio;
                _context.RegistroNotasCreditos.Add(item);

                var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Id == item.ProductoId)
                    ?? throw new DomainException($"Producto con ID '{item.ProductoId}' no encontrado.");
                producto.Cantidad = Math.Max(0, producto.Cantidad - item.Cantidad);
                _context.Productos.Update(producto);
            }
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return nota;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new DomainException($"Error al crear la nota de crédito: {ex.Message}", ex);
        }
    }

    public async Task AddItemsAsync(int numcliente, int ncfolio, List<RegistroNotasCredito> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var nota = await GetByFolioAsync(numcliente, ncfolio)
            ?? throw new DomainException($"Nota de crédito {numcliente}/{ncfolio} no encontrada.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var currentMaxDetalle = await _context.RegistroNotasCreditos
                .Where(r => r.Numcliente == numcliente && r.Ncfolio == ncfolio)
                .MaxAsync(r => (int?)r.Detalle) ?? 0;

            decimal batchTotal = 0;
            int detalleCounter = currentMaxDetalle + 1;

            foreach (var item in items)
            {
                item.Numcliente = numcliente;
                item.Ncfolio = ncfolio;
                item.Detalle = detalleCounter++;
                _context.RegistroNotasCreditos.Add(item);
                batchTotal += item.Importe;

                var producto = await _context.Productos.FirstOrDefaultAsync(p => p.Id == item.ProductoId)
                    ?? throw new DomainException($"Producto con ID '{item.ProductoId}' no encontrado.");
                producto.Cantidad = Math.Max(0, producto.Cantidad - item.Cantidad);
                _context.Productos.Update(producto);
            }

            nota.Monto += batchTotal;
            _context.NotasCreditos.Update(nota);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new DomainException($"Error al agregar items: {ex.Message}", ex);
        }
    }

    public async Task<PagosNotasCredito> RegistrarPagoAsync(int numcliente, int ncfolio, decimal montoPagado, Venta ventaPago, RegistroVenta registroVentaPago)
    {
        var nota = await GetByFolioAsync(numcliente, ncfolio)
            ?? throw new DomainException($"Nota de crédito {numcliente}/{ncfolio} no encontrada.");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            decimal remaining = nota.Monto - nota.MontoPagado;
            decimal efectivoPagado = Math.Min(montoPagado, remaining);

            // Create venta record for this payment
            _context.Venta.Add(ventaPago);
            await _context.SaveChangesAsync();

            registroVentaPago.IdVenta = ventaPago.IdVenta;
            _context.RegistroVentas.Add(registroVentaPago);

            // Record the payment
            var nextPago = await _context.PagosNotasCreditos
                .Where(p => p.Numcliente == numcliente && p.Ncfolio == ncfolio)
                .MaxAsync(p => (int?)p.Pago) ?? 0;

            var pago = new PagosNotasCredito
            {
                Numcliente = numcliente,
                Ncfolio = ncfolio,
                Pago = nextPago + 1,
                Importe = efectivoPagado,
                Fecha = DateTime.Now
            };
            _context.PagosNotasCreditos.Add(pago);

            nota.MontoPagado += efectivoPagado;
            nota.Status = nota.MontoPagado >= nota.Monto ? "CO" : "PC";
            _context.NotasCreditos.Update(nota);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return pago;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new DomainException($"Error al registrar el pago: {ex.Message}", ex);
        }
    }

    public async Task CancelarAsync(int numcliente, int ncfolio)
    {
        var nota = await GetByFolioAsync(numcliente, ncfolio)
            ?? throw new DomainException($"Nota de crédito {numcliente}/{ncfolio} no encontrada.");

        if (nota.Status != "AU")
            throw new DomainException("Solo se pueden cancelar notas con estatus 'AU' (Autorizado).");

        nota.Status = "CA";
        _context.NotasCreditos.Update(nota);
        await _context.SaveChangesAsync();
    }
}
