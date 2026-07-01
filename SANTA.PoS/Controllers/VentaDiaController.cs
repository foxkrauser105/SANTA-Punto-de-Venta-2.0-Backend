using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SANTA.PoS.Business.DTOs;
using SANTA.PoS.Business.Services;

namespace SANTA.PoS.Controllers;

[Route("api/venta-dia")]
[ApiController]
[Authorize]
public class VentaDiaController(VentaDiaService ventaDiaService) : ControllerBase
{
    private readonly VentaDiaService _ventaDiaService = ventaDiaService;

    [HttpGet("{fecha}")]
    public async Task<IActionResult> GetByFecha(DateOnly fecha)
    {
        var ventaDia = await _ventaDiaService.GetByFechaAsync(fecha);
        if (ventaDia is null) return NotFound(new { message = $"No hay registro para la fecha {fecha}." });
        return Ok(ventaDia);
    }

    [HttpGet("{fecha}/venta-pos")]
    public async Task<IActionResult> GetVentaPos(DateOnly fecha)
    {
        var total = await _ventaDiaService.GetVentaPosAsync(fecha);
        return Ok(total);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVentaDiaDto dto)
    {
        var created = await _ventaDiaService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetByFecha), new { fecha = created.Fecha }, created);
    }

    [HttpPut("{fecha}")]
    public async Task<IActionResult> Update(DateOnly fecha, [FromBody] UpdateVentaDiaDto dto)
    {
        var updated = await _ventaDiaService.UpdateAsync(fecha, dto);
        return Ok(updated);
    }
}
