using AutoMapper;
using SANTA.PoS.Business.DTOs;
using SANTA.PoS.Business.Interfaces;
using SANTA.PoS.Domain.Entities;
using SANTA.PoS.Domain.Exceptions;

namespace SANTA.PoS.Business.Services;

public class VentaDiaService(IVentaDiaRepository repository, IMapper mapper)
{
    private readonly IVentaDiaRepository _repository = repository;
    private readonly IMapper _mapper = mapper;

    public async Task<VentaDiaDto?> GetByFechaAsync(DateOnly fecha)
    {
        var ventaDia = await _repository.GetByFechaAsync(fecha);
        return ventaDia is null ? null : _mapper.Map<VentaDiaDto>(ventaDia);
    }

    public async Task<VentaPosDto> GetVentaPosAsync(DateOnly fecha)
    {
        var total = await _repository.GetVentaPosAsync(fecha);
        return new VentaPosDto(total);
    }

    public async Task<VentaDiaDto> CreateAsync(CreateVentaDiaDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var existing = await _repository.GetByFechaAsync(dto.Fecha);
        if (existing is not null)
            throw new DomainException($"Ya existe un registro de venta del día para la fecha {dto.Fecha}.");

        var ventaDia = BuildVentaDia(dto);
        await _repository.CreateAsync(ventaDia);
        return _mapper.Map<VentaDiaDto>(ventaDia);
    }

    public async Task<VentaDiaDto> UpdateAsync(DateOnly fecha, UpdateVentaDiaDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var ventaDia = await _repository.GetByFechaAsync(fecha)
            ?? throw new DomainException($"No existe registro de venta del día para la fecha {fecha}.");

        if (dto.Inicio.HasValue) ventaDia.Inicio = dto.Inicio.Value;
        if (dto.Monedas.HasValue) ventaDia.Monedas = dto.Monedas.Value;
        if (dto.UsoMonedas.HasValue) ventaDia.UsoMonedas = dto.UsoMonedas.Value;
        if (dto.Proveedores.HasValue) ventaDia.Proveedores = dto.Proveedores.Value;
        if (dto.Gasto.HasValue) ventaDia.Gasto = dto.Gasto.Value;
        if (dto.SaldoInicial.HasValue) ventaDia.SaldoInicial = dto.SaldoInicial.Value;
        if (dto.SaldoFinal.HasValue) ventaDia.SaldoFinal = dto.SaldoFinal.Value;
        if (dto.Caja.HasValue) ventaDia.Caja = dto.Caja.Value;
        if (dto.VentaAbarrote.HasValue) ventaDia.VentaAbarrote = dto.VentaAbarrote.Value;

        RecalculateDerivedFields(ventaDia);
        await _repository.UpdateAsync(ventaDia);
        return _mapper.Map<VentaDiaDto>(ventaDia);
    }

    private static VentaDia BuildVentaDia(CreateVentaDiaDto dto)
    {
        var vd = new VentaDia
        {
            Fecha = dto.Fecha,
            Inicio = dto.Inicio,
            Monedas = dto.Monedas,
            UsoMonedas = dto.UsoMonedas,
            Proveedores = dto.Proveedores,
            Gasto = dto.Gasto,
            SaldoInicial = dto.SaldoInicial,
            SaldoFinal = dto.SaldoFinal,
            Caja = dto.Caja,
            VentaAbarrote = dto.VentaAbarrote,
            VentaSaldo = dto.SaldoInicial - dto.SaldoFinal
        };
        RecalculateDerivedFields(vd);
        return vd;
    }

    private static void RecalculateDerivedFields(VentaDia vd)
    {
        vd.Quedo = vd.Proveedores - vd.Gasto;
        vd.VentaSaldo = vd.SaldoInicial - vd.SaldoFinal;
        vd.Total = vd.Monedas + vd.VentaSaldo + vd.VentaAbarrote + vd.Caja;
        vd.Final = vd.Monedas + vd.SaldoFinal + vd.VentaSaldo + vd.VentaAbarrote + vd.Caja;
    }
}
