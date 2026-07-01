namespace SANTA.PoS.Business.DTOs;

public record UsuarioDto(
    string Usuclave,
    string Nombre,
    string APaterno,
    string? AMaterno,
    string? Telefono,
    DateTime FechaAlta,
    int Status,
    string Rol
);

public record CreateUsuarioDto(
    string Usuclave,
    string Nombre,
    string APaterno,
    string? AMaterno,
    string? Telefono,
    string Password,
    string Rol
);

public record UpdateUsuarioDto(
    string? Nombre,
    string? APaterno,
    string? AMaterno,
    string? Telefono
);

public record ChangePasswordDto(
    string? OldPassword,
    string NewPassword
);

public record LoginDto(
    string Usuclave,
    string Password
);

public record LoginResponseDto(
    string Token,
    string Usuclave,
    string Nombre,
    string Rol
);
