namespace SANTA.PoS.Domain.Entities;

public partial class Descuento
{
    public string IdProducto { get; set; } = null!;

    public double CantidadMinima { get; set; }

    public decimal? PrecioDescuento { get; set; }

    public int Status { get; set; }

    public virtual Producto IdProductoNavigation { get; set; } = null!;
}
