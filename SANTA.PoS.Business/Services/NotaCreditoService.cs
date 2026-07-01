using AutoMapper;
using SANTA.PoS.Business.DTOs;
using SANTA.PoS.Business.Interfaces;
using SANTA.PoS.Domain.Entities;
using SANTA.PoS.Domain.Exceptions;

namespace SANTA.PoS.Business.Services;

public class NotaCreditoService(
    INotaCreditoRepository repository,
    IProductRepository productRepository,
    IClienteRepository clienteRepository,
    IMapper mapper)
{
    private readonly INotaCreditoRepository _repository = repository;
    private readonly IProductRepository _productRepository = productRepository;
    private readonly IClienteRepository _clienteRepository = clienteRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<NotaCreditoDto>> GetByClienteAsync(int numcliente)
    {
        var notas = await _repository.GetByClienteAsync(numcliente);
        return _mapper.Map<IEnumerable<NotaCreditoDto>>(notas);
    }

    public async Task<NotaCreditoDto> GetByFolioAsync(int numcliente, int ncfolio)
    {
        var nota = await _repository.GetByFolioAsync(numcliente, ncfolio)
            ?? throw new DomainException($"Nota de crédito {numcliente}/{ncfolio} no encontrada.");
        return _mapper.Map<NotaCreditoDto>(nota);
    }

    public async Task<NotaCreditoDto?> GetOpenByClienteAsync(int numcliente)
    {
        var nota = await _repository.GetOpenByClienteAsync(numcliente);
        return nota is null ? null : _mapper.Map<NotaCreditoDto>(nota);
    }

    public async Task<int> GetNextFolioAsync(int numcliente) =>
        await _repository.GetNextFolioAsync(numcliente);

    public async Task<IEnumerable<PagoNotaCreditoDto>> GetPagosAsync(int numcliente, int ncfolio)
    {
        var pagos = await _repository.GetPagosAsync(numcliente, ncfolio);
        return _mapper.Map<IEnumerable<PagoNotaCreditoDto>>(pagos);
    }

    public async Task<NotaCreditoDto> CreateAsync(CreateNotaCreditoDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (dto.Items is null || dto.Items.Count == 0)
            throw new DomainException("La nota de crédito debe contener al menos un producto.");

        var cliente = await _clienteRepository.GetByIdAsync(dto.Numcliente)
            ?? throw new DomainException($"Cliente con número '{dto.Numcliente}' no encontrado.");

        var openNota = await _repository.GetOpenByClienteAsync(dto.Numcliente);
        if (openNota is not null)
        {
            if (openNota.FechaCompromiso < DateOnly.FromDateTime(DateTime.Today))
                throw new DomainException("El cliente tiene una nota de crédito vencida. Debe liquidarse antes de crear una nueva.");
            throw new DomainException("El cliente ya tiene una nota de crédito abierta.");
        }

        if (dto.FechaCompromiso <= DateOnly.FromDateTime(DateTime.Today))
            throw new DomainException("La fecha de compromiso debe ser posterior a la fecha actual.");

        decimal monto = 0;
        var items = new List<RegistroNotasCredito>();
        int detalleCounter = 1;

        foreach (var itemDto in dto.Items)
        {
            var producto = await _productRepository.GetByIdAsync(itemDto.IdProducto)
                ?? throw new DomainException($"Producto '{itemDto.IdProducto}' no encontrado.");

            items.Add(new RegistroNotasCredito
            {
                ProductoId = producto.Id,
                Cantidad = itemDto.Cantidad,
                Precio = itemDto.Precio,
                Importe = itemDto.Importe,
                Descuento = itemDto.Descuento,
                Detalle = detalleCounter++,
                FechaSurtido = DateTime.Now
            });
            monto += itemDto.Importe;
        }

        var nota = new NotasCredito
        {
            Numcliente = dto.Numcliente,
            Ncfolio = dto.Ncfolio,
            Status = "AU",
            FechaAlta = DateTime.Now,
            FechaCompromiso = dto.FechaCompromiso,
            Monto = monto,
            MontoPagado = 0
        };

        await _repository.CreateWithItemsAsync(nota, items);
        return _mapper.Map<NotaCreditoDto>(nota);
    }

    public async Task AddItemsAsync(int numcliente, int ncfolio, AddItemsNotaCreditoDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var nota = await _repository.GetByFolioAsync(numcliente, ncfolio)
            ?? throw new DomainException($"Nota de crédito {numcliente}/{ncfolio} no encontrada.");

        if (nota.Status is "CO" or "CA")
            throw new DomainException("No se pueden agregar items a una nota cobrada o cancelada.");

        var items = new List<RegistroNotasCredito>();
        foreach (var itemDto in dto.Items)
        {
            var producto = await _productRepository.GetByIdAsync(itemDto.IdProducto)
                ?? throw new DomainException($"Producto '{itemDto.IdProducto}' no encontrado.");

            items.Add(new RegistroNotasCredito
            {
                ProductoId = producto.Id,
                Cantidad = itemDto.Cantidad,
                Precio = itemDto.Precio,
                Importe = itemDto.Importe,
                Descuento = itemDto.Descuento,
                FechaSurtido = DateTime.Now
            });
        }

        await _repository.AddItemsAsync(numcliente, ncfolio, items);
    }

    public async Task<PagoNotaCreditoDto> RegistrarPagoAsync(int numcliente, int ncfolio, CreatePagoNotaCreditoDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var nota = await _repository.GetByFolioAsync(numcliente, ncfolio)
            ?? throw new DomainException($"Nota de crédito {numcliente}/{ncfolio} no encontrada.");

        if (nota.Status is "CO" or "CA")
            throw new DomainException("La nota de crédito ya está cobrada o cancelada.");

        decimal remaining = nota.Monto - nota.MontoPagado;
        bool isFullPayment = dto.MontoPagado >= remaining;

        // NC01 = full payment, NC02 = partial payment
        var pseudoProductoCode = isFullPayment ? "NC01" : "NC02";
        var pseudoProducto = await _productRepository.GetByIdAsync(pseudoProductoCode)
            ?? throw new DomainException($"Producto especial '{pseudoProductoCode}' no configurado en la base de datos.");

        decimal efectivoPagado = Math.Min(dto.MontoPagado, remaining);

        var ventaPago = new Venta
        {
            ImporteVenta = efectivoPagado,
            Fecha = DateOnly.FromDateTime(DateTime.Today)
        };

        var registroVentaPago = new RegistroVenta
        {
            ProductoId = pseudoProducto.Id,
            Precio = efectivoPagado,
            Cantidad = 1,
            Descuento = false,
            Numcliente = numcliente,
            Ncfolio = ncfolio
        };

        var pago = await _repository.RegistrarPagoAsync(numcliente, ncfolio, dto.MontoPagado, ventaPago, registroVentaPago);
        return _mapper.Map<PagoNotaCreditoDto>(pago);
    }

    public async Task CancelarAsync(int numcliente, int ncfolio)
    {
        await _repository.CancelarAsync(numcliente, ncfolio);
    }
}
