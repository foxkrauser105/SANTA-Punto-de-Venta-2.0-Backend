namespace SANTA.PoS.Domain.Entities;

public partial class RegistroVenta
{
    public int IdRegistro { get; set; }

    public int IdVenta { get; set; }

    public int ProductoId { get; set; }

    public decimal? Precio { get; set; }

    public double Cantidad { get; set; }

    public bool Descuento { get; set; }

    public int? Numcliente { get; set; }

    public int? Ncfolio { get; set; }

    public virtual Producto Producto { get; set; } = null!;

    public virtual Venta IdVentaNavigation { get; set; } = null!;
}
