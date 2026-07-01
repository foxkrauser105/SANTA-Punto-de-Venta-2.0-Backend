namespace SANTA.PoS.Domain.Entities;

public partial class Venta
{
    public int IdVenta { get; set; }

    public decimal ImporteVenta { get; set; }

    public DateOnly Fecha { get; set; }

    public virtual ICollection<RegistroVenta> RegistroVentas { get; set; } = new List<RegistroVenta>();
}
