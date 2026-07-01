namespace SANTA.PoS.Business.DTOs;

public record DescuentoDto(
    string IdProducto,
    double CantidadMinima,
    decimal? PrecioDescuento,
    int Status
);

public record CreateDescuentoDto(
    string IdProducto,
    double CantidadMinima,
    decimal PrecioDescuento
);

public record UpdateDescuentoDto(
    double CantidadMinima,
    decimal PrecioDescuento
);
