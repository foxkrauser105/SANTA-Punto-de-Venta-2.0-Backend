using AutoMapper;
using SANTA.PoS.Business.DTOs;
using SANTA.PoS.Business.Interfaces;
using SANTA.PoS.Domain.Entities;
using SANTA.PoS.Domain.Exceptions;

namespace SANTA.PoS.Business.Services;

public class NotificacionService(INotificacionRepository repository, IMapper mapper)
{
    private readonly INotificacionRepository _repository = repository;
    private readonly IMapper _mapper = mapper;

    public async Task<IEnumerable<NotificacionDto>> GetFilteredAsync(NotificacionFilterDto filter)
    {
        var notificaciones = await _repository.GetFilteredAsync(
            filter.IncluirInactivosTerminados ?? false,
            filter.Tipo,
            filter.Prioridad,
            filter.Status,
            filter.FechaAlta,
            filter.FechaTermino,
            filter.IdMensaje);
        return _mapper.Map<IEnumerable<NotificacionDto>>(notificaciones);
    }

    public async Task<NotificacionDto> GetByIdAsync(int id)
    {
        var notificacion = await _repository.GetByIdAsync(id)
            ?? throw new DomainException($"Notificación con ID '{id}' no encontrada.");
        return _mapper.Map<NotificacionDto>(notificacion);
    }

    public async Task<NotificacionDto> CreateAsync(CreateNotificacionDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var notificacion = new Notificacione
        {
            Tipo = dto.Tipo,
            Prioridad = dto.Prioridad,
            Status = 1,
            Mensaje = dto.Mensaje,
            FechaAlta = DateTime.Now,
            IdProducto = dto.IdProducto,
            Usuclave = dto.Usuclave,
            UsuclaveRecibe = dto.UsuclaveRecibe
        };

        await _repository.CreateAsync(notificacion);
        return _mapper.Map<NotificacionDto>(notificacion);
    }

    public async Task InactivarAsync(int id)
    {
        var notificacion = await _repository.GetByIdAsync(id)
            ?? throw new DomainException($"Notificación con ID '{id}' no encontrada.");

        if (notificacion.Status != 1)
            throw new DomainException("Solo se pueden inactivar notificaciones con estatus Activo.");

        notificacion.Status = 0;
        notificacion.FechaTermino = DateTime.Now;
        await _repository.UpdateAsync(notificacion);
    }

    public async Task TerminarAsync(int id)
    {
        var notificacion = await _repository.GetByIdAsync(id)
            ?? throw new DomainException($"Notificación con ID '{id}' no encontrada.");

        if (notificacion.Status != 1)
            throw new DomainException("Solo se pueden terminar notificaciones con estatus Activo.");

        notificacion.Status = 2;
        notificacion.FechaTermino = DateTime.Now;
        await _repository.UpdateAsync(notificacion);
    }
}
