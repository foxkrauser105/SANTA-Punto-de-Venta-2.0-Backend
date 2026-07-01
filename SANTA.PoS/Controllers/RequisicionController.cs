using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SANTA.PoS.Business.DTOs;
using SANTA.PoS.Business.Services;
using System.Security.Claims;

namespace SANTA.PoS.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RequisicionController(RequisicionService requisicionService) : ControllerBase
{
    private readonly RequisicionService _requisicionService = requisicionService;

    private string CurrentUsuclave =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException("Usuario no autenticado.");

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var items = await _requisicionService.GetAsync(CurrentUsuclave);
        return Ok(items);
    }

    [HttpPut]
    public async Task<IActionResult> Save([FromBody] SaveRequisicionDto dto)
    {
        await _requisicionService.SaveAsync(CurrentUsuclave, dto);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Clear()
    {
        await _requisicionService.ClearAsync(CurrentUsuclave);
        return NoContent();
    }

    [HttpPost("enviar")]
    public async Task<IActionResult> Enviar()
    {
        await _requisicionService.EnviarAsync(CurrentUsuclave);
        return NoContent();
    }
}
