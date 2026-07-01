namespace SANTA.PoS.Business.DTOs;

public record RequisicionItemDto(
    string IdProducto,
    string NombreProducto,
    double Cantidad
);

public record SaveRequisicionDto(
    List<SaveRequisicionItemDto> Items
);

public record SaveRequisicionItemDto(
    string IdProducto,
    double Cantidad
);
