using AutoMapper;
using SANTA.PoS.Business.DTOs;
using SANTA.PoS.Business.Interfaces;
using SANTA.PoS.Domain.Entities;
using SANTA.PoS.Domain.Exceptions;

namespace SANTA.PoS.Business.Services;

public class DescuentoService(IDescuentoRepository repository, IProductRepository productRepository, IMapper mapper)
{
    private readonly IDescuentoRepository _repository = repository;
    private readonly IProductRepository _productRepository = productRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<DescuentoDto>> GetAllAsync()
    {
        var descuentos = await _repository.GetAllWithProductoAsync();
        return _mapper.Map<IEnumerable<DescuentoDto>>(descuentos);
    }

    public async Task<DescuentoDto> GetByIdProductoAsync(string idProducto)
    {
        var descuento = await _repository.GetByIdProductoAsync(idProducto)
            ?? throw new DomainException($"No existe descuento para el producto '{idProducto}'.");
        return _mapper.Map<DescuentoDto>(descuento);
    }

    public async Task<DescuentoDto> CreateAsync(CreateDescuentoDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var producto = await _productRepository.GetByIdAsync(dto.IdProducto)
            ?? throw new DomainException($"Producto '{dto.IdProducto}' no encontrado.");

        var existing = await _repository.GetByIdProductoAsync(dto.IdProducto);
        if (existing is not null)
            throw new DomainException($"El producto '{dto.IdProducto}' ya tiene un descuento registrado.");

        var descuento = new Descuento
        {
            ProductoId = producto.Id,
            CantidadMinima = dto.CantidadMinima,
            PrecioDescuento = dto.PrecioDescuento,
            Status = 1
        };

        await _repository.CreateAsync(descuento);

        var created = await _repository.GetByIdProductoAsync(dto.IdProducto)
            ?? throw new DomainException("Error al crear el descuento.");
        return _mapper.Map<DescuentoDto>(created);
    }

    public async Task UpdateAsync(string idProducto, UpdateDescuentoDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var descuento = await _repository.GetByIdProductoAsync(idProducto)
            ?? throw new DomainException($"No existe descuento para el producto '{idProducto}'.");

        descuento.CantidadMinima = dto.CantidadMinima;
        descuento.PrecioDescuento = dto.PrecioDescuento;

        await _repository.UpdateAsync(descuento);
    }

    public async Task ToggleStatusAsync(string idProducto)
    {
        var descuento = await _repository.GetByIdProductoAsync(idProducto)
            ?? throw new DomainException($"No existe descuento para el producto '{idProducto}'.");

        descuento.Status = descuento.Status == 1 ? 0 : 1;
        await _repository.UpdateAsync(descuento);
    }

    public async Task DeleteAsync(string idProducto)
    {
        var descuento = await _repository.GetByIdProductoAsync(idProducto)
            ?? throw new DomainException($"No existe descuento para el producto '{idProducto}'.");

        await _repository.DeleteByIdProductoAsync(idProducto);
    }
}
