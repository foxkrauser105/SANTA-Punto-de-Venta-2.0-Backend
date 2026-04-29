namespace SANTA.PoS.Domain.Entities;

public partial class RegistroNotasCredito
{
    public int Numcliente { get; set; }

    public int Ncfolio { get; set; }

    public int Detalle { get; set; }

    public string IdProducto { get; set; } = null!;

    public double Cantidad { get; set; }

    public decimal Precio { get; set; }

    public decimal Importe { get; set; }

    public DateTime FechaSurtido { get; set; }

    public int Descuento { get; set; }

    public virtual Producto IdProductoNavigation { get; set; } = null!;
}
