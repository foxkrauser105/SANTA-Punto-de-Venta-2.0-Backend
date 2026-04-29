namespace SANTA.PoS.Domain.Entities;

public partial class NotasCredito
{
    public int Numcliente { get; set; }

    public int Ncfolio { get; set; }

    public string Status { get; set; } = null!;

    public DateTime FechaAlta { get; set; }

    public DateOnly FechaCompromiso { get; set; }

    public decimal Monto { get; set; }

    public decimal MontoPagado { get; set; }
}
