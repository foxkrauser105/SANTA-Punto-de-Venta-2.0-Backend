using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SANTA.PoS.Business.DTOs;
using SANTA.PoS.Business.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SANTA.PoS.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(UsuarioService usuarioService, IConfiguration configuration) : ControllerBase
{
    private readonly UsuarioService _usuarioService = usuarioService;
    private readonly IConfiguration _configuration = configuration;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var usuario = await _usuarioService.ValidateCredentialsAsync(dto.Usuclave, dto.Password);
        if (usuario is null)
            return Unauthorized(new { message = "Credenciales inválidas o usuario inactivo." });

        var token = GenerateJwtToken(usuario.Usuclave, usuario.Nombre, usuario.Rol);
        return Ok(new LoginResponseDto(token, usuario.Usuclave, usuario.Nombre, usuario.Rol));
    }

    private string GenerateJwtToken(string usuclave, string nombre, string rol)
    {
        var key = _configuration["Jwt:Key"]!;
        var issuer = _configuration["Jwt:Issuer"]!;
        var audience = _configuration["Jwt:Audience"]!;
        var expiresMinutes = int.Parse(_configuration["Jwt:ExpiresInMinutes"] ?? "480");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuclave),
            new Claim(ClaimTypes.Name, nombre),
            new Claim(ClaimTypes.Role, rol)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
