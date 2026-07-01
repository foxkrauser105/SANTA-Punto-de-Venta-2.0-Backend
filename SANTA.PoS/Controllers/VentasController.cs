using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SANTA.PoS.Business.DTOs;
using SANTA.PoS.Business.Services;

namespace SANTA.PoS.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class VentasController(VentaService ventaService) : ControllerBase
{
    private readonly VentaService _ventaService = ventaService;

    [HttpGet]
    public async Task<IActionResult> GetAllVentas()
    {
        var ventas = await _ventaService.GetAllVentasAsync();
        return Ok(ventas);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetVentaById(int id)
    {
        var venta = await _ventaService.GetVentaByIdAsync(id);
        return Ok(venta);
    }

    [HttpGet("by-fecha/{fecha}")]
    public async Task<IActionResult> GetVentasByFecha(DateOnly fecha)
    {
        var ventas = await _ventaService.GetVentasByFechaAsync(fecha);
        return Ok(ventas);
    }

    [HttpGet("by-fecha-range")]
    public async Task<IActionResult> GetVentasByFechaRange([FromQuery] DateOnly fechaInicio, [FromQuery] DateOnly fechaFin)
    {
        var ventas = await _ventaService.GetVentasByFechaRangeAsync(fechaInicio, fechaFin);
        return Ok(ventas);
    }

    [HttpPost]
    public async Task<IActionResult> CreateVenta([FromBody] CreateVentaDto venta)
    {
        var createdVenta = await _ventaService.CreateVentaAsync(venta);
        return CreatedAtAction(nameof(GetVentaById), new { id = createdVenta.IdVenta }, createdVenta);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVenta(int id, [FromBody] UpdateVentaDto venta)
    {
        await _ventaService.UpdateVentaAsync(id, venta);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteVenta(int id)
    {
        await _ventaService.DeleteVentaAsync(id);
        return NoContent();
    }

    [HttpDelete("{id}/detalle/{idRegistro:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteLineaVenta(int id, int idRegistro)
    {
        await _ventaService.DeleteLineaVentaAsync(id, idRegistro);
        return NoContent();
    }
}
