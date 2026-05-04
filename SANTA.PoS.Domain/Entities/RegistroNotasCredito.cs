namespace SANTA.PoS.Domain.Entities;

public partial class RegistroNotasCredito
{
    public int Numcliente { get; set; }

    public int Ncfolio { get; set; }

    public int Detalle { get; set; }

    public int ProductoId { get; set; }

    public double Cantidad { get; set; }

    public decimal Precio { get; set; }

    public decimal Importe { get; set; }

    public DateTime FechaSurtido { get; set; }

    public int Descuento { get; set; }

    public virtual Producto Producto { get; set; } = null!;
}
