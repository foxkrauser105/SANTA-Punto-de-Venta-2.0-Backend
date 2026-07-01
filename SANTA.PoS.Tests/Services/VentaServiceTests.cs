using Moq;
using Xunit;
using SANTA.PoS.Business.DTOs;
using SANTA.PoS.Business.Interfaces;
using SANTA.PoS.Business.Services;
using SANTA.PoS.Domain.Entities;
using SANTA.PoS.Domain.Exceptions;
using SANTA.PoS.Tests.Helpers;

namespace SANTA.PoS.Tests.Services;

public class VentaServiceTests
{
    private readonly Mock<IVentaRepository> _ventaRepoMock = new();
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly VentaService _sut;

    public VentaServiceTests()
    {
        _sut = new VentaService(_ventaRepoMock.Object, _productRepoMock.Object, MapperFactory.Create());
    }

    private static CreateVentaDto VentaWithItems(List<CreateRegistroVentaDto>? items = null) =>
        new() { ImporteVenta = 500m, Fecha = DateOnly.FromDateTime(DateTime.Today), RegistroVentas = items ?? [new CreateRegistroVentaDto { IdProducto = "P001", Precio = 25m, Cantidad = 2, Descuento = false }] };

    // ── CreateVentaAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task CreateVentaAsync_EmptyRegistros_ThrowsDomainException()
    {
        var dto = new CreateVentaDto { ImporteVenta = 0m, Fecha = DateOnly.FromDateTime(DateTime.Today), RegistroVentas = [] };

        await Assert.ThrowsAsync<DomainException>(
            () => _sut.CreateVentaAsync(dto));
    }

    [Fact]
    public async Task CreateVentaAsync_NullRegistros_ThrowsDomainException()
    {
        var dto = new CreateVentaDto { ImporteVenta = 0m, Fecha = DateOnly.FromDateTime(DateTime.Today), RegistroVentas = null };

        await Assert.ThrowsAsync<DomainException>(
            () => _sut.CreateVentaAsync(dto));
    }

    [Fact]
    public async Task CreateVentaAsync_ProductNotFound_ThrowsDomainException()
    {
        _productRepoMock.Setup(r => r.GetByIdAsync("P001")).ReturnsAsync((Producto?)null);

        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _sut.CreateVentaAsync(VentaWithItems()));

        Assert.Contains("P001", ex.Message);
    }

    [Fact]
    public async Task CreateVentaAsync_ValidVenta_CallsCreateWithDetalles()
    {
        var producto = new Producto { Id = 1, IdProducto = "P001" };
        _productRepoMock.Setup(r => r.GetByIdAsync("P001")).ReturnsAsync(producto);

        var ventaReturnada = new Venta
        {
            IdVenta = 1,
            ImporteVenta = 500m,
            Fecha = DateOnly.FromDateTime(DateTime.Today),
            RegistroVentas = [new RegistroVenta { IdRegistro = 1, ProductoId = 1, Producto = producto }]
        };
        _ventaRepoMock
            .Setup(r => r.CreateVentaWithDetallesAsync(It.IsAny<Venta>(), It.IsAny<List<RegistroVenta>>()))
            .ReturnsAsync(ventaReturnada);

        var result = await _sut.CreateVentaAsync(VentaWithItems());

        Assert.Equal(1, result.IdVenta);
        _ventaRepoMock.Verify(r => r.CreateVentaWithDetallesAsync(It.IsAny<Venta>(), It.IsAny<List<RegistroVenta>>()), Times.Once);
    }

    // ── GetVentaByIdAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetVentaByIdAsync_NonExistentId_ThrowsDomainException()
    {
        _ventaRepoMock.Setup(r => r.GetVentaWithDetallesAsync(99)).ReturnsAsync((Venta?)null);

        await Assert.ThrowsAsync<DomainException>(
            () => _sut.GetVentaByIdAsync(99));
    }

    [Fact]
    public async Task GetVentaByIdAsync_ExistingId_ReturnsDto()
    {
        var venta = new Venta { IdVenta = 5, ImporteVenta = 200m, Fecha = DateOnly.FromDateTime(DateTime.Today) };
        _ventaRepoMock.Setup(r => r.GetVentaWithDetallesAsync(5)).ReturnsAsync(venta);

        var result = await _sut.GetVentaByIdAsync(5);

        Assert.Equal(5, result!.IdVenta);
        Assert.Equal(200m, result.ImporteVenta);
    }
}
