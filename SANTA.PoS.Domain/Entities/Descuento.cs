namespace SANTA.PoS.Domain.Entities;

public partial class Descuento
{
    public int Id { get; set; }

    public int ProductoId { get; set; }

    public double CantidadMinima { get; set; }

    public decimal? PrecioDescuento { get; set; }

    public int Status { get; set; }

    public virtual Producto Producto { get; set; } = null!;
}
