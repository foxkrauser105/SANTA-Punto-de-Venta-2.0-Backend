namespace SANTA.PoS.Business.DTOs;

public record VentaDiaDto(
    int IdVentadia,
    DateOnly Fecha,
    decimal Inicio,
    decimal Monedas,
    decimal UsoMonedas,
    decimal Proveedores,
    decimal Gasto,
    decimal Quedo,
    decimal SaldoInicial,
    decimal SaldoFinal,
    decimal Caja,
    decimal VentaSaldo,
    decimal VentaAbarrote,
    decimal Total,
    decimal Final
);

public record CreateVentaDiaDto(
    DateOnly Fecha,
    decimal Inicio,
    decimal Monedas,
    decimal UsoMonedas,
    decimal Proveedores,
    decimal Gasto,
    decimal SaldoInicial,
    decimal SaldoFinal,
    decimal Caja,
    decimal VentaAbarrote
);

public record UpdateVentaDiaDto(
    decimal? Inicio,
    decimal? Monedas,
    decimal? UsoMonedas,
    decimal? Proveedores,
    decimal? Gasto,
    decimal? SaldoInicial,
    decimal? SaldoFinal,
    decimal? Caja,
    decimal? VentaAbarrote
);

public record VentaPosDto(decimal VentaAbarroteCalculada);
