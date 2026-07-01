namespace SANTA.PoS.Business.DTOs;

public record NotificacionDto(
    int IdMensaje,
    string Tipo,
    string Prioridad,
    int Status,
    string Mensaje,
    DateTime FechaAlta,
    DateTime? FechaTermino,
    string? IdProducto,
    string? Usuclave,
    string? UsuclaveRecibe
);

public record CreateNotificacionDto(
    string Tipo,
    string Prioridad,
    string Mensaje,
    string? IdProducto,
    string? Usuclave,
    string? UsuclaveRecibe
);

public record NotificacionFilterDto(
    bool? IncluirInactivosTerminados,
    string? Tipo,
    string? Prioridad,
    int? Status,
    DateTime? FechaAlta,
    DateTime? FechaTermino,
    int? IdMensaje
);
