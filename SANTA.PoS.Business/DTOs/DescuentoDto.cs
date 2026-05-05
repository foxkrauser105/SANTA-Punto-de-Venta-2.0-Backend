namespace SANTA.PoS.Business.DTOs;

public record DescuentoDto(
    int Id,
    double CantidadMinima,
    decimal? PrecioDescuento,
    int Status
);
