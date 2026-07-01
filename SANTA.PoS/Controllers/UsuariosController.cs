using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SANTA.PoS.Business.DTOs;
using SANTA.PoS.Business.Services;
using System.Security.Claims;

namespace SANTA.PoS.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsuariosController(UsuarioService usuarioService) : ControllerBase
{
    private readonly UsuarioService _usuarioService = usuarioService;

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var usuarios = await _usuarioService.GetAllAsync();
        return Ok(usuarios);
    }

    [HttpGet("{usuclave}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(string usuclave)
    {
        var usuario = await _usuarioService.GetByIdAsync(usuclave);
        return Ok(usuario);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateUsuarioDto dto)
    {
        var created = await _usuarioService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { usuclave = created.Usuclave }, created);
    }

    [HttpPut("{usuclave}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(string usuclave, [FromBody] UpdateUsuarioDto dto)
    {
        await _usuarioService.UpdateAsync(usuclave, dto);
        return NoContent();
    }

    [HttpPatch("{usuclave}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleStatus(string usuclave)
    {
        await _usuarioService.ToggleStatusAsync(usuclave);
        return NoContent();
    }

    [HttpPut("{usuclave}/password")]
    public async Task<IActionResult> ChangePassword(string usuclave, [FromBody] ChangePasswordDto dto)
    {
        var callerUsuclave = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        if (!isAdmin && callerUsuclave != usuclave)
            return Forbid();

        await _usuarioService.ChangePasswordAsync(usuclave, dto, isAdmin);
        return NoContent();
    }
}
