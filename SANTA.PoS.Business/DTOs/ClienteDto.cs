namespace SANTA.PoS.Business.DTOs;

public record ClienteDto(
    int Numcliente,
    string Nombre,
    string APaterno,
    string? AMaterno,
    string Calle,
    int NumeroExt,
    int? NumeroInt,
    string Colonia,
    string Telefono,
    string UsuclaveUltAct
);

public record CreateClienteDto(
    int Numcliente,
    string Nombre,
    string APaterno,
    string? AMaterno,
    string Calle,
    int NumeroExt,
    int? NumeroInt,
    string Colonia,
    string Telefono,
    string UsuclaveUltAct
);

public record UpdateClienteDto(
    string? Nombre,
    string? APaterno,
    string? AMaterno,
    string? Calle,
    int? NumeroExt,
    int? NumeroInt,
    string? Colonia,
    string? Telefono,
    string UsuclaveUltAct
);
