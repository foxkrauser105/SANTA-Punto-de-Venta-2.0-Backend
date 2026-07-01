using Moq;
using Xunit;
using SANTA.PoS.Business.DTOs;
using SANTA.PoS.Business.Interfaces;
using SANTA.PoS.Business.Services;
using SANTA.PoS.Domain.Entities;
using SANTA.PoS.Domain.Exceptions;
using SANTA.PoS.Tests.Helpers;

namespace SANTA.PoS.Tests.Services;

public class ClienteServiceTests
{
    private readonly Mock<IClienteRepository> _repoMock = new();
    private readonly ClienteService _sut;

    public ClienteServiceTests()
    {
        _sut = new ClienteService(_repoMock.Object, MapperFactory.Create());
    }

    private static CreateClienteDto ValidDto(string telefono = "5512345678") =>
        new(1, "Juan", "Pérez", null, "Av. Principal", 10, null, "Centro", telefono, "admin");

    // ── CreateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidDto_CallsRepositoryCreate()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Cliente?)null);
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Cliente>())).ReturnsAsync(new Cliente());

        await _sut.CreateAsync(ValidDto());

        _repoMock.Verify(r => r.CreateAsync(It.IsAny<Cliente>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_TelefonoTooShort_ThrowsDomainException()
    {
        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _sut.CreateAsync(ValidDto("12345")));

        Assert.Contains("10", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_DuplicateNumcliente_ThrowsDomainException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Cliente { Numcliente = 1 });

        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _sut.CreateAsync(ValidDto()));

        Assert.Contains("1", ex.Message);
    }

    // ── GetByIdAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingCliente_ReturnsDto()
    {
        var cliente = new Cliente { Numcliente = 5, Nombre = "María", APaterno = "López", Calle = "Calle 5", NumeroExt = 1, Colonia = "Centro", Telefono = "5500000000", UsuclaveUltAct = "admin" };
        _repoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(cliente);

        var result = await _sut.GetByIdAsync(5);

        Assert.Equal(5, result.Numcliente);
        Assert.Equal("María", result.Nombre);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentCliente_ThrowsDomainException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Cliente?)null);

        await Assert.ThrowsAsync<DomainException>(
            () => _sut.GetByIdAsync(99));
    }

    // ── UpdateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_NonExistentCliente_ThrowsDomainException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Cliente?)null);

        await Assert.ThrowsAsync<DomainException>(
            () => _sut.UpdateAsync(99, new UpdateClienteDto(null, null, null, null, null, null, null, null, "admin")));
    }

    [Fact]
    public async Task UpdateAsync_ShortTelefono_ThrowsDomainException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Cliente { Numcliente = 1 });

        var ex = await Assert.ThrowsAsync<DomainException>(
            () => _sut.UpdateAsync(1, new UpdateClienteDto(null, null, null, null, null, null, null, "123", "admin")));

        Assert.Contains("10", ex.Message);
    }
}
