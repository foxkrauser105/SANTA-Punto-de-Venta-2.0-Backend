using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SANTA.PoS.Business.DTOs;
using SANTA.PoS.Business.Services;

namespace SANTA.PoS.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ClientesController(ClienteService clienteService) : ControllerBase
{
    private readonly ClienteService _clienteService = clienteService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var clientes = await _clienteService.GetAllAsync();
        return Ok(clientes);
    }

    [HttpGet("{numCliente:int}")]
    public async Task<IActionResult> GetById(int numCliente)
    {
        var cliente = await _clienteService.GetByIdAsync(numCliente);
        return Ok(cliente);
    }

    [HttpGet("next-folio")]
    public async Task<IActionResult> GetNextFolio()
    {
        var folio = await _clienteService.GetNextFolioAsync();
        return Ok(new { nextFolio = folio });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClienteDto dto)
    {
        var created = await _clienteService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { numCliente = created.Numcliente }, created);
    }

    [HttpPut("{numCliente:int}")]
    public async Task<IActionResult> Update(int numCliente, [FromBody] UpdateClienteDto dto)
    {
        await _clienteService.UpdateAsync(numCliente, dto);
        return NoContent();
    }
}
