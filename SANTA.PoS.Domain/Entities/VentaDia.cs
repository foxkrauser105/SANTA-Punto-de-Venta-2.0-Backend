namespace SANTA.PoS.Domain.Entities;

public partial class VentaDia
{
    public int IdVentadia { get; set; }

    public decimal Inicio { get; set; }

    public decimal Monedas { get; set; }

    public decimal UsoMonedas { get; set; }

    public decimal Proveedores { get; set; }

    public decimal Gasto { get; set; }

    public decimal Quedo { get; set; }

    public decimal SaldoInicial { get; set; }

    public decimal SaldoFinal { get; set; }

    public decimal Caja { get; set; }

    public decimal VentaSaldo { get; set; }

    public decimal VentaAbarrote { get; set; }

    public decimal Total { get; set; }

    public decimal Final { get; set; }

    public DateOnly Fecha { get; set; }
}
