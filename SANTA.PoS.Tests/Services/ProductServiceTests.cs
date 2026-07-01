using Moq;
using Xunit;
using SANTA.PoS.Business.DTOs;
using SANTA.PoS.Business.Interfaces;
using SANTA.PoS.Business.Services;
using SANTA.PoS.Domain.Entities;
using SANTA.PoS.Domain.Exceptions;
using SANTA.PoS.Tests.Helpers;

namespace SANTA.PoS.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repoMock = new();
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _sut = new ProductService(_repoMock.Object, MapperFactory.Create());
    }

    // ── GetProductByIdAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetProductByIdAsync_ExistingProduct_ReturnsDto()
    {
        var product = new Producto { IdProducto = "P001", Nombre = "Leche", Marca = "Lala", Categoria = "Lácteos", Precio = 25m, Cantidad = 10, Status = 1, Fechaultact = DateTime.Now };
        _repoMock.Setup(r => r.GetByIdAsync("P001")).ReturnsAsync(product);

        var result = await _sut.GetProductByIdAsync("P001");

        Assert.NotNull(result);
        Assert.Equal("P001", result!.IdProducto);
        Assert.Equal("Leche", result.Nombre);
    }

    [Fact]
    public async Task GetProductByIdAsync_NonExistentProduct_ThrowsDomainException()
    {
        _repoMock.Setup(r => r.GetByIdAsync("NOPE")).ReturnsAsync((Producto?)null);

        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _sut.GetProductByIdAsync("NOPE"));

        Assert.Contains("NOPE", ex.Message);
    }

    // ── GetPrecioParaVentaAsync ────────────────────────────────────────────

    [Fact]
    public async Task GetPrecioParaVentaAsync_NonExistentProduct_ThrowsDomainException()
    {
        _repoMock.Setup(r => r.GetByIdAsync("NOPE")).ReturnsAsync((Producto?)null);

        await Assert.ThrowsAsync<DomainException>(
            () => _sut.GetPrecioParaVentaAsync("NOPE", 1));
    }

    [Fact]
    public async Task GetPrecioParaVentaAsync_NoDiscount_ReturnsRegularPrice()
    {
        var product = new Producto { IdProducto = "P001", Precio = 30m, Descuento = null };
        _repoMock.Setup(r => r.GetByIdAsync("P001")).ReturnsAsync(product);

        var result = await _sut.GetPrecioParaVentaAsync("P001", 5);

        Assert.Equal(30m, result.Precio);
        Assert.False(result.AplicaDescuento);
    }

    [Fact]
    public async Task GetPrecioParaVentaAsync_ActiveDiscountAndQuantityMet_ReturnsDiscountPrice()
    {
        var product = new Producto
        {
            IdProducto = "P001",
            Precio = 30m,
            Descuento = new Descuento { Status = 1, CantidadMinima = 3, PrecioDescuento = 20m }
        };
        _repoMock.Setup(r => r.GetByIdAsync("P001")).ReturnsAsync(product);

        var result = await _sut.GetPrecioParaVentaAsync("P001", 5);

        Assert.Equal(20m, result.Precio);
        Assert.True(result.AplicaDescuento);
    }

    [Fact]
    public async Task GetPrecioParaVentaAsync_ActiveDiscountButQuantityBelowMinimum_ReturnsRegularPrice()
    {
        var product = new Producto
        {
            IdProducto = "P001",
            Precio = 30m,
            Descuento = new Descuento { Status = 1, CantidadMinima = 5, PrecioDescuento = 20m }
        };
        _repoMock.Setup(r => r.GetByIdAsync("P001")).ReturnsAsync(product);

        var result = await _sut.GetPrecioParaVentaAsync("P001", 3);

        Assert.Equal(30m, result.Precio);
        Assert.False(result.AplicaDescuento);
    }

    [Fact]
    public async Task GetPrecioParaVentaAsync_InactiveDiscount_ReturnsRegularPrice()
    {
        var product = new Producto
        {
            IdProducto = "P001",
            Precio = 30m,
            Descuento = new Descuento { Status = 0, CantidadMinima = 1, PrecioDescuento = 15m }
        };
        _repoMock.Setup(r => r.GetByIdAsync("P001")).ReturnsAsync(product);

        var result = await _sut.GetPrecioParaVentaAsync("P001", 5);

        Assert.Equal(30m, result.Precio);
        Assert.False(result.AplicaDescuento);
    }

    // ── RenameCodigoAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task RenameCodigoAsync_ValidNewCode_CallsUpdateMethod()
    {
        var product = new Producto { IdProducto = "P001" };
        _repoMock.Setup(r => r.GetByIdAsync("P001")).ReturnsAsync(product);
        _repoMock.Setup(r => r.ExistsByIdProductoAsync("P002")).ReturnsAsync(false);

        await _sut.RenameCodigoAsync("P001", "P002");

        _repoMock.Verify(r => r.UpdateIdProductoAsync("P001", "P002"), Times.Once);
    }

    [Fact]
    public async Task RenameCodigoAsync_NonExistentProduct_ThrowsDomainException()
    {
        _repoMock.Setup(r => r.GetByIdAsync("NOPE")).ReturnsAsync((Producto?)null);

        await Assert.ThrowsAsync<DomainException>(
            () => _sut.RenameCodigoAsync("NOPE", "P002"));
    }

    [Fact]
    public async Task RenameCodigoAsync_DuplicateNewCode_ThrowsDomainException()
    {
        var product = new Producto { IdProducto = "P001" };
        _repoMock.Setup(r => r.GetByIdAsync("P001")).ReturnsAsync(product);
        _repoMock.Setup(r => r.ExistsByIdProductoAsync("P002")).ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _sut.RenameCodigoAsync("P001", "P002"));

        Assert.Contains("P002", ex.Message);
    }
}
