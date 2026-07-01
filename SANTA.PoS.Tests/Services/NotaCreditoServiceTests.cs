using Moq;
using Xunit;
using SANTA.PoS.Business.DTOs;
using SANTA.PoS.Business.Interfaces;
using SANTA.PoS.Business.Services;
using SANTA.PoS.Domain.Entities;
using SANTA.PoS.Domain.Exceptions;
using SANTA.PoS.Tests.Helpers;

namespace SANTA.PoS.Tests.Services;

public class NotaCreditoServiceTests
{
    private readonly Mock<INotaCreditoRepository> _repoMock = new();
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<IClienteRepository> _clienteRepoMock = new();
    private readonly NotaCreditoService _sut;

    public NotaCreditoServiceTests()
    {
        _sut = new NotaCreditoService(
            _repoMock.Object,
            _productRepoMock.Object,
            _clienteRepoMock.Object,
            MapperFactory.Create());
    }

    private static CreateNotaCreditoDto DtoWithItems(DateOnly? fechaCompromiso = null) =>
        new(
            Numcliente: 1,
            Ncfolio: 1,
            FechaCompromiso: fechaCompromiso ?? DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            Items: [new CreateNotaCreditoItemDto("P001", 2, 25m, 50m, false)]
        );

    private static CreateNotaCreditoDto DtoWithNoItems() =>
        new(1, 1, DateOnly.FromDateTime(DateTime.Today.AddDays(5)), []);

    // ── CreateAsync validations ────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_EmptyItems_ThrowsDomainException()
    {
        await Assert.ThrowsAsync<DomainException>(
            () => _sut.CreateAsync(DtoWithNoItems()));
    }

    [Fact]
    public async Task CreateAsync_NonExistentCliente_ThrowsDomainException()
    {
        _clienteRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Cliente?)null);

        await Assert.ThrowsAsync<DomainException>(
            () => _sut.CreateAsync(DtoWithItems()));
    }

    [Fact]
    public async Task CreateAsync_ClienteHasOpenNota_ThrowsDomainException()
    {
        _clienteRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Cliente());
        _repoMock.Setup(r => r.GetOpenByClienteAsync(1))
            .ReturnsAsync(new NotasCredito
            {
                FechaCompromiso = DateOnly.FromDateTime(DateTime.Today.AddDays(10)),
                Status = "AU"
            });

        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _sut.CreateAsync(DtoWithItems()));

        Assert.Contains("abierta", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_ClienteHasExpiredOpenNota_ThrowsDomainExceptionWithExpiredMessage()
    {
        _clienteRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Cliente());
        _repoMock.Setup(r => r.GetOpenByClienteAsync(1))
            .ReturnsAsync(new NotasCredito
            {
                FechaCompromiso = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
                Status = "AU"
            });

        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _sut.CreateAsync(DtoWithItems()));

        Assert.Contains("vencida", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_FechaCompromisoIsToday_ThrowsDomainException()
    {
        _clienteRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Cliente());
        _repoMock.Setup(r => r.GetOpenByClienteAsync(1)).ReturnsAsync((NotasCredito?)null);

        await Assert.ThrowsAsync<DomainException>(
            () => _sut.CreateAsync(DtoWithItems(DateOnly.FromDateTime(DateTime.Today))));
    }

    [Fact]
    public async Task CreateAsync_ValidDto_CallsRepository()
    {
        _clienteRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Cliente());
        _repoMock.Setup(r => r.GetOpenByClienteAsync(1)).ReturnsAsync((NotasCredito?)null);
        _productRepoMock.Setup(r => r.GetByIdAsync("P001")).ReturnsAsync(
            new Producto { Id = 10, IdProducto = "P001", Nombre = "Pan" });
        _repoMock.Setup(r => r.CreateWithItemsAsync(It.IsAny<NotasCredito>(), It.IsAny<List<RegistroNotasCredito>>()))
            .ReturnsAsync(new NotasCredito { Numcliente = 1, Ncfolio = 1, Status = "AU", FechaAlta = DateTime.Now, FechaCompromiso = DateOnly.FromDateTime(DateTime.Today.AddDays(5)), Monto = 50m });

        await _sut.CreateAsync(DtoWithItems());

        _repoMock.Verify(r => r.CreateWithItemsAsync(It.IsAny<NotasCredito>(), It.IsAny<List<RegistroNotasCredito>>()), Times.Once);
    }

    // ── RegistrarPagoAsync validations ─────────────────────────────────────

    [Fact]
    public async Task RegistrarPagoAsync_ClosedNota_ThrowsDomainException()
    {
        _repoMock.Setup(r => r.GetByFolioAsync(1, 1))
            .ReturnsAsync(new NotasCredito { Status = "CO", Monto = 100m, MontoPagado = 100m });

        await Assert.ThrowsAsync<DomainException>(
            () => _sut.RegistrarPagoAsync(1, 1, new CreatePagoNotaCreditoDto(50m)));
    }

    [Fact]
    public async Task RegistrarPagoAsync_CancelledNota_ThrowsDomainException()
    {
        _repoMock.Setup(r => r.GetByFolioAsync(1, 1))
            .ReturnsAsync(new NotasCredito { Status = "CA", Monto = 100m, MontoPagado = 0m });

        await Assert.ThrowsAsync<DomainException>(
            () => _sut.RegistrarPagoAsync(1, 1, new CreatePagoNotaCreditoDto(50m)));
    }

    [Fact]
    public async Task RegistrarPagoAsync_PartialPayment_UsesNC02Product()
    {
        _repoMock.Setup(r => r.GetByFolioAsync(1, 1))
            .ReturnsAsync(new NotasCredito { Status = "AU", Monto = 200m, MontoPagado = 0m });

        _productRepoMock.Setup(r => r.GetByIdAsync("NC02"))
            .ReturnsAsync(new Producto { Id = 2, IdProducto = "NC02" });

        _repoMock.Setup(r => r.RegistrarPagoAsync(
            1, 1, 50m, It.IsAny<Venta>(), It.IsAny<RegistroVenta>()))
            .ReturnsAsync(new PagosNotasCredito { Numcliente = 1, Ncfolio = 1, Pago = 1, Importe = 50m });

        await _sut.RegistrarPagoAsync(1, 1, new CreatePagoNotaCreditoDto(50m));

        _productRepoMock.Verify(r => r.GetByIdAsync("NC02"), Times.Once);
        _productRepoMock.Verify(r => r.GetByIdAsync("NC01"), Times.Never);
    }

    [Fact]
    public async Task RegistrarPagoAsync_FullPayment_UsesNC01Product()
    {
        _repoMock.Setup(r => r.GetByFolioAsync(1, 1))
            .ReturnsAsync(new NotasCredito { Status = "AU", Monto = 100m, MontoPagado = 0m });

        _productRepoMock.Setup(r => r.GetByIdAsync("NC01"))
            .ReturnsAsync(new Producto { Id = 1, IdProducto = "NC01" });

        _repoMock.Setup(r => r.RegistrarPagoAsync(
            1, 1, 100m, It.IsAny<Venta>(), It.IsAny<RegistroVenta>()))
            .ReturnsAsync(new PagosNotasCredito { Numcliente = 1, Ncfolio = 1, Pago = 1, Importe = 100m });

        await _sut.RegistrarPagoAsync(1, 1, new CreatePagoNotaCreditoDto(100m));

        _productRepoMock.Verify(r => r.GetByIdAsync("NC01"), Times.Once);
        _productRepoMock.Verify(r => r.GetByIdAsync("NC02"), Times.Never);
    }
}
