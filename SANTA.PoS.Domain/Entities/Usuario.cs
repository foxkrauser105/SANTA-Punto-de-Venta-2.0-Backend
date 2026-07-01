namespace SANTA.PoS.Domain.Entities;

public partial class Usuario
{
    public string Usuclave { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string APaterno { get; set; } = null!;

    public string? AMaterno { get; set; }

    public byte[]? Pass { get; set; }

    public DateTime FechaAlta { get; set; }

    public int Status { get; set; }

    public string? Telefono { get; set; }

    public DateTime FechaUltAct { get; set; }

    public string Rol { get; set; } = "User";

    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();
}
