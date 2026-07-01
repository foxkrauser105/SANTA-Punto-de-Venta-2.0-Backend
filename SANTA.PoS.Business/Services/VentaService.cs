using AutoMapper;
using SANTA.PoS.Business.DTOs;
using SANTA.PoS.Business.Interfaces;
using SANTA.PoS.Domain.Entities;
using SANTA.PoS.Domain.Exceptions;

namespace SANTA.PoS.Business.Services;

public class VentaService(IVentaRepository ventaRepository, IProductRepository productRepository, IMapper mapper)
{
    private readonly IVentaRepository _ventaRepository = ventaRepository;
    private readonly IProductRepository _productRepository = productRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<VentaDto> CreateVentaAsync(CreateVentaDto createVentaDto)
    {
        ArgumentNullException.ThrowIfNull(createVentaDto);

        if (createVentaDto.RegistroVentas is null || createVentaDto.RegistroVentas.Count == 0)
        {
            throw new DomainException("La venta debe contener al menos un registro de venta.");
        }

        var venta = new Venta
        {
            ImporteVenta = createVentaDto.ImporteVenta,
            Fecha = createVentaDto.Fecha
        };

        // Convert DTOs to entities and resolve ProductoId from IdProducto
        var registroVentas = new List<RegistroVenta>();

        foreach (var registroDto in createVentaDto.RegistroVentas)
        {
            var producto = await _productRepository.GetByIdAsync(registroDto.IdProducto);
            if (producto is null)
            {
                throw new DomainException($"Producto con código de barras '{registroDto.IdProducto}' no encontrado.");
            }

            var registroVenta = new RegistroVenta
            {
                ProductoId = producto.Id,
                Precio = registroDto.Precio,
                Cantidad = registroDto.Cantidad,
                Descuento = registroDto.Descuento,
                Numcliente = registroDto.Numcliente,
                Ncfolio = registroDto.Ncfolio
            };

            registroVentas.Add(registroVenta);
        }

        var ventaCreated = await _ventaRepository.CreateVentaWithDetallesAsync(venta, registroVentas);

        return _mapper.Map<VentaDto>(ventaCreated);
    }

    public async Task<VentaDto?> GetVentaByIdAsync(int id)
    {
        var venta = await _ventaRepository.GetVentaWithDetallesAsync(id);

        if (venta is null)
        {
            throw new DomainException($"Venta con ID '{id}' no encontrada.");
        }

        return _mapper.Map<VentaDto>(venta);
    }

    public async Task<IEnumerable<VentaDto>> GetAllVentasAsync()
    {
        var ventas = await _ventaRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<VentaDto>>(ventas);
    }

    public async Task<IEnumerable<VentaDto>> GetVentasByFechaAsync(DateOnly fecha)
    {
        var ventas = await _ventaRepository.GetFilteredVentasAsync(v => v.Fecha == fecha);
        return _mapper.Map<IEnumerable<VentaDto>>(ventas);
    }

    public async Task<IEnumerable<VentaDto>> GetVentasByFechaRangeAsync(DateOnly fechaInicio, DateOnly fechaFin)
    {
        var ventas = await _ventaRepository.GetFilteredVentasAsync(v => v.Fecha >= fechaInicio && v.Fecha <= fechaFin);
        return _mapper.Map<IEnumerable<VentaDto>>(ventas);
    }

    public async Task UpdateVentaAsync(int id, UpdateVentaDto updatedVenta)
    {
        ArgumentNullException.ThrowIfNull(updatedVenta);

        var venta = await _ventaRepository.GetByIdAsync(id);
        if (venta is null)
        {
            throw new DomainException($"Venta con ID '{id}' no encontrada.");
        }

        if (updatedVenta.ImporteVenta.HasValue)
        {
            venta.ImporteVenta = updatedVenta.ImporteVenta.Value;
        }

        if (updatedVenta.Fecha.HasValue)
        {
            venta.Fecha = updatedVenta.Fecha.Value;
        }

        await _ventaRepository.UpdateAsync(venta);
    }

    public async Task DeleteVentaAsync(int id)
    {
        var venta = await _ventaRepository.GetByIdAsync(id);
        if (venta is null)
        {
            throw new DomainException($"Venta con ID '{id}' no encontrada.");
        }

        await _ventaRepository.DeleteAsync(id);
    }

    public async Task DeleteLineaVentaAsync(int ventaId, int idRegistro)
    {
        await _ventaRepository.DeleteLineaVentaAsync(ventaId, idRegistro);
    }
}
