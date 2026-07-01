using SANTA.PoS.Business.DTOs;
using SANTA.PoS.Business.Interfaces;
using SANTA.PoS.Domain.Entities;
using SANTA.PoS.Domain.Exceptions;

namespace SANTA.PoS.Business.Services;

public class RequisicionService(IRequisicionRepository repository, IProductRepository productRepository, IEmailService emailService)
{
    private readonly IRequisicionRepository _repository = repository;
    private readonly IProductRepository _productRepository = productRepository;
    private readonly IEmailService _emailService = emailService;

    public async Task<IEnumerable<RequisicionItemDto>> GetAsync(string usuclave)
    {
        var items = await _repository.GetByUsuarioAsync(usuclave);
        return items.Select(i => new RequisicionItemDto(i.Producto.IdProducto, i.Producto.Nombre, i.Cantidad));
    }

    public async Task SaveAsync(string usuclave, SaveRequisicionDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var items = new List<RequisicionItem>();
        foreach (var itemDto in dto.Items)
        {
            var producto = await _productRepository.GetByIdAsync(itemDto.IdProducto)
                ?? throw new DomainException($"Producto '{itemDto.IdProducto}' no encontrado.");

            items.Add(new RequisicionItem
            {
                ProductoId = producto.Id,
                Cantidad = itemDto.Cantidad
            });
        }

        await _repository.SaveAsync(usuclave, items);
    }

    public async Task ClearAsync(string usuclave)
    {
        await _repository.ClearAsync(usuclave);
    }

    public async Task EnviarAsync(string usuclave)
    {
        var items = await _repository.GetByUsuarioAsync(usuclave);
        var list = items.ToList();

        if (!list.Any())
            throw new DomainException("La requisición está vacía.");

        var rows = list.Select(i =>
            $"<tr><td>{i.Producto.Nombre}</td><td>{i.Cantidad}</td></tr>");

        var body = $"""
            <h2>Requisición de productos</h2>
            <table border='1' cellpadding='5' cellspacing='0'>
              <thead><tr><th>Nombre</th><th>Cantidad</th></tr></thead>
              <tbody>{string.Join("", rows)}</tbody>
            </table>
            """;

        await _emailService.SendAsync("Requisición de productos", body);
        await _repository.ClearAsync(usuclave);
    }
}
