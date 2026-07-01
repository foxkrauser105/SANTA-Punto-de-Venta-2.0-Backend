using Moq;
using Xunit;
using SANTA.PoS.Business.DTOs;
using SANTA.PoS.Business.Interfaces;
using SANTA.PoS.Business.Services;
using SANTA.PoS.Domain.Entities;
using SANTA.PoS.Domain.Exceptions;
using SANTA.PoS.Tests.Helpers;

namespace SANTA.PoS.Tests.Services;

public class VentaDiaServiceTests
{
    private readonly Mock<IVentaDiaRepository> _repoMock = new();
    private readonly VentaDiaService _sut;

    public VentaDiaServiceTests()
    {
        _sut = new VentaDiaService(_repoMock.Object, MapperFactory.Create());
    }

    // ── CreateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_DuplicateDate_ThrowsDomainException()
    {
        var fecha = new DateOnly(2026, 6, 21);
        _repoMock.Setup(r => r.GetByFechaAsync(fecha)).ReturnsAsync(new VentaDia());

        var dto = new CreateVentaDiaDto(fecha, 100, 50, 10, 200, 30, 500, 400, 150, 0);

        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _sut.CreateAsync(dto));

        Assert.Contains(fecha.ToString(), ex.Message);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_CalculatesAllDerivedFieldsCorrectly()
    {
        var fecha = new DateOnly(2026, 6, 21);
        _repoMock.Setup(r => r.GetByFechaAsync(fecha)).ReturnsAsync((VentaDia?)null);

        VentaDia? captured = null;
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<VentaDia>()))
            .Callback<VentaDia>(vd => captured = vd)
            .ReturnsAsync(new VentaDia());

        // Arrange: known values for easy manual verification
        // Proveedores=200, Gasto=30   → Quedo=170
        // SaldoInicial=500, SaldoFinal=400 → VentaSaldo=100
        // VentaAbarrote=0 (not auto, from dto)
        // Total = Monedas(50) + VentaSaldo(100) + VentaAbarrote(0) + Caja(150) = 300
        // Final = Monedas(50) + SaldoFinal(400) + VentaSaldo(100) + VentaAbarrote(0) + Caja(150) = 700
        var dto = new CreateVentaDiaDto(
            Fecha: fecha, Inicio: 100, Monedas: 50, UsoMonedas: 10,
            Proveedores: 200, Gasto: 30,
            SaldoInicial: 500, SaldoFinal: 400,
            Caja: 150, VentaAbarrote: 0);

        await _sut.CreateAsync(dto);

        Assert.NotNull(captured);
        Assert.Equal(170m, captured!.Quedo);
        Assert.Equal(100m, captured.VentaSaldo);
        Assert.Equal(300m, captured.Total);
        Assert.Equal(700m, captured.Final);
    }

    // ── UpdateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_NonExistentDate_ThrowsDomainException()
    {
        var fecha = new DateOnly(2026, 1, 1);
        _repoMock.Setup(r => r.GetByFechaAsync(fecha)).ReturnsAsync((VentaDia?)null);

        await Assert.ThrowsAsync<DomainException>(
            () => _sut.UpdateAsync(fecha, new UpdateVentaDiaDto(null, null, null, null, null, null, null, null, null)));
    }

    [Fact]
    public async Task UpdateAsync_PartialUpdate_RecalculatesDerivedFields()
    {
        var fecha = new DateOnly(2026, 6, 21);
        var existing = new VentaDia
        {
            Fecha = fecha,
            Monedas = 50, VentaSaldo = 0, VentaAbarrote = 0, Caja = 0,
            Proveedores = 100, Gasto = 20, SaldoInicial = 300, SaldoFinal = 200
        };
        _repoMock.Setup(r => r.GetByFechaAsync(fecha)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<VentaDia>())).Returns(Task.CompletedTask);

        // Update only Gasto: 20 → 50
        var result = await _sut.UpdateAsync(fecha, new UpdateVentaDiaDto(
            Inicio: null, Monedas: null, UsoMonedas: null,
            Proveedores: null, Gasto: 50m,
            SaldoInicial: null, SaldoFinal: null,
            Caja: null, VentaAbarrote: null));

        // Quedo = Proveedores(100) - Gasto(50) = 50
        Assert.Equal(50m, result.Quedo);
    }
}
