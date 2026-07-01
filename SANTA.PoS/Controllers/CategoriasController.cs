using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SANTA.PoS.Business.Services;

namespace SANTA.PoS.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CategoriasController(CategoriaService categoriaService) : ControllerBase
{
    private readonly CategoriaService _categoriaService = categoriaService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categorias = await _categoriaService.GetAllAsync();
        return Ok(categorias);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] string nombre)
    {
        var categoria = await _categoriaService.CreateAsync(nombre);
        return CreatedAtAction(nameof(GetAll), categoria);
    }

    [HttpDelete("{nombre}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string nombre)
    {
        await _categoriaService.DeleteAsync(nombre);
        return NoContent();
    }
}
