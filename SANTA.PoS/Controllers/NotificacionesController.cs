using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SANTA.PoS.Business.DTOs;
using SANTA.PoS.Business.Services;

namespace SANTA.PoS.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class NotificacionesController(NotificacionService notificacionService) : ControllerBase
{
    private readonly NotificacionService _notificacionService = notificacionService;

    [HttpGet]
    public async Task<IActionResult> GetFiltered(
        [FromQuery] bool? incluirInactivosTerminados,
        [FromQuery] string? tipo,
        [FromQuery] string? prioridad,
        [FromQuery] int? status,
        [FromQuery] DateTime? fechaAlta,
        [FromQuery] DateTime? fechaTermino,
        [FromQuery] int? idMensaje)
    {
        var filter = new NotificacionFilterDto(incluirInactivosTerminados, tipo, prioridad, status, fechaAlta, fechaTermino, idMensaje);
        var notificaciones = await _notificacionService.GetFilteredAsync(filter);
        return Ok(notificaciones);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var notificacion = await _notificacionService.GetByIdAsync(id);
        return Ok(notificacion);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNotificacionDto dto)
    {
        var created = await _notificacionService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.IdMensaje }, created);
    }

    [HttpPatch("{id:int}/inactivar")]
    public async Task<IActionResult> Inactivar(int id)
    {
        await _notificacionService.InactivarAsync(id);
        return NoContent();
    }

    [HttpPatch("{id:int}/terminar")]
    public async Task<IActionResult> Terminar(int id)
    {
        await _notificacionService.TerminarAsync(id);
        return NoContent();
    }
}
