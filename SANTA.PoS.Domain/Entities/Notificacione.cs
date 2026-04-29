namespace SANTA.PoS.Domain.Entities;

public partial class Notificacione
{
    public int IdMensaje { get; set; }

    public string Tipo { get; set; } = null!;

    public string Prioridad { get; set; } = null!;

    public int Status { get; set; }

    public string Mensaje { get; set; } = null!;

    public DateTime FechaAlta { get; set; }

    public DateTime? FechaTermino { get; set; }

    public string? IdProducto { get; set; }

    public string? Usuclave { get; set; }

    public string? UsuclaveRecibe { get; set; }
}
