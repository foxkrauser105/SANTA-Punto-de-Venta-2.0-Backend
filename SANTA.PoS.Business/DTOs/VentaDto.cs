namespace SANTA.PoS.Business.DTOs;

public record CreateVentaDto
{
    public decimal ImporteVenta { get; init; }
    public DateOnly Fecha { get; init; }
    public List<CreateRegistroVentaDto>? RegistroVentas { get; init; }
}

public record UpdateVentaDto
{
    public decimal? ImporteVenta { get; init; }
    public DateOnly? Fecha { get; init; }
}

public record VentaDto
{
    public int IdVenta { get; init; }
    public decimal ImporteVenta { get; init; }
    public DateOnly Fecha { get; init; }
    public List<RegistroVentaDto>? RegistroVentas { get; init; }
}

public record CreateRegistroVentaDto
{
    public string IdProducto { get; init; } = null!;
    public decimal? Precio { get; init; }
    public double Cantidad { get; init; }
    public bool Descuento { get; init; }
    public int? Numcliente { get; init; }
    public int? Ncfolio { get; init; }
}

public record UpdateRegistroVentaDto
{
    public string IdProducto { get; init; } = null!;
    public decimal? Precio { get; init; }
    public double Cantidad { get; init; }
    public bool Descuento { get; init; }
    public int? Numcliente { get; init; }
    public int? Ncfolio { get; init; }
}

public record RegistroVentaDto
{
    public string IdProducto { get; init; } = null!;
    public decimal? Precio { get; init; }
    public double Cantidad { get; init; }
    public bool Descuento { get; init; }
    public int? Numcliente { get; init; }
    public int? Ncfolio { get; init; }
}
