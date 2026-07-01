using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SANTA.PoS.Business.DTOs;
using SANTA.PoS.Business.Services;

namespace SANTA.PoS.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DescuentosController(DescuentoService descuentoService) : ControllerBase
{
    private readonly DescuentoService _descuentoService = descuentoService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var descuentos = await _descuentoService.GetAllAsync();
        return Ok(descuentos);
    }

    [HttpGet("{idProducto}")]
    public async Task<IActionResult> GetByIdProducto(string idProducto)
    {
        var descuento = await _descuentoService.GetByIdProductoAsync(idProducto);
        return Ok(descuento);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateDescuentoDto dto)
    {
        var created = await _descuentoService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetByIdProducto), new { idProducto = created.IdProducto }, created);
    }

    [HttpPut("{idProducto}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(string idProducto, [FromBody] UpdateDescuentoDto dto)
    {
        await _descuentoService.UpdateAsync(idProducto, dto);
        return NoContent();
    }

    [HttpPatch("{idProducto}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleStatus(string idProducto)
    {
        await _descuentoService.ToggleStatusAsync(idProducto);
        return NoContent();
    }

    [HttpDelete("{idProducto}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string idProducto)
    {
        await _descuentoService.DeleteAsync(idProducto);
        return NoContent();
    }
}
