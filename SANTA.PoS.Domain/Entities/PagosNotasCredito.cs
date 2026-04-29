namespace SANTA.PoS.Domain.Entities;

public partial class PagosNotasCredito
{
    public int Numcliente { get; set; }

    public int Ncfolio { get; set; }

    public int Pago { get; set; }

    public decimal Importe { get; set; }

    public DateTime Fecha { get; set; }
}
