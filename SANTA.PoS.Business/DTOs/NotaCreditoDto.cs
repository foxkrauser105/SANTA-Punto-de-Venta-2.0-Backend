namespace SANTA.PoS.Business.DTOs;

public record NotaCreditoDto(
    int Numcliente,
    int Ncfolio,
    string Status,
    DateTime FechaAlta,
    DateOnly FechaCompromiso,
    decimal Monto,
    decimal MontoPagado,
    List<RegistroNotaCreditoDto>? Items = null
);

public record RegistroNotaCreditoDto(
    string IdProducto,
    double Cantidad,
    decimal Precio,
    decimal Importe,
    bool Descuento,
    DateTime FechaSurtido
);

public record PagoNotaCreditoDto(
    int Numcliente,
    int Ncfolio,
    int Pago,
    decimal Importe,
    DateTime Fecha
);

public record CreateNotaCreditoDto(
    int Numcliente,
    int Ncfolio,
    DateOnly FechaCompromiso,
    List<CreateNotaCreditoItemDto> Items
);

public record CreateNotaCreditoItemDto(
    string IdProducto,
    double Cantidad,
    decimal Precio,
    decimal Importe,
    bool Descuento
);

public record AddItemsNotaCreditoDto(
    List<CreateNotaCreditoItemDto> Items
);

public record CreatePagoNotaCreditoDto(
    decimal MontoPagado
);
