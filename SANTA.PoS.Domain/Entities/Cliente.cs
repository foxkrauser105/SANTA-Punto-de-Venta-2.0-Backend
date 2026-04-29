namespace SANTA.PoS.Domain.Entities;

public partial class Cliente
{
    public int Numcliente { get; set; }

    public string Nombre { get; set; } = null!;

    public string APaterno { get; set; } = null!;

    public string? AMaterno { get; set; }

    public string Calle { get; set; } = null!;

    public int NumeroExt { get; set; }

    public int? NumeroInt { get; set; }

    public string Colonia { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string UsuclaveUltAct { get; set; } = null!;

    public virtual Usuario UsuclaveUltActNavigation { get; set; } = null!;
}
