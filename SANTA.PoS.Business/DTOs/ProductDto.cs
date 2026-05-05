namespace SANTA.PoS.Business.DTOs;

public record CreateUpdateProductDto(
    string? IdProducto,
    string? Nombre,
    string? Marca,
    string? Categoria,
    double? Cantidad,
    decimal? Precio,
    int? Status,
    DateTime? Fechaultact,
    DescuentoDto? Descuento
);

public record ProductDto(
    string IdProducto,
    string Nombre,
    string Marca,
    string Categoria,
    double Cantidad,
    decimal Precio,
    int Status,
    DateTime Fechaultact,
    DescuentoDto? Descuento
);
