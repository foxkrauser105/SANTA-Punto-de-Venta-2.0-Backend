namespace SANTA.PoS.Domain.Entities;

public partial class RequisicionItem
{
    public string Usuclave { get; set; } = null!;

    public int ProductoId { get; set; }

    public double Cantidad { get; set; }

    public virtual Producto Producto { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;
}
