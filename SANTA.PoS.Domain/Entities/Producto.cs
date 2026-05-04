namespace SANTA.PoS.Domain.Entities;

public partial class Producto
{
    public int Id { get; set; }

    public string IdProducto { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Marca { get; set; } = null!;

    public string Categoria { get; set; } = null!;

    public double Cantidad { get; set; }

    public decimal Precio { get; set; }

    public int Status { get; set; }

    public DateTime Fechaultact { get; set; }

    public virtual Descuento? Descuento { get; set; }

    public virtual ICollection<RegistroNotasCredito> RegistroNotasCreditos { get; set; } = new List<RegistroNotasCredito>();

    public virtual ICollection<RegistroVenta> RegistroVenta { get; set; } = new List<RegistroVenta>();
}
