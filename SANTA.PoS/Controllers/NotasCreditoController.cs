using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SANTA.PoS.Business.DTOs;
using SANTA.PoS.Business.Services;

namespace SANTA.PoS.Controllers;

[Route("api/notas-credito")]
[ApiController]
[Authorize]
public class NotasCreditoController(NotaCreditoService notaCreditoService) : ControllerBase
{
    private readonly NotaCreditoService _notaCreditoService = notaCreditoService;

    [HttpGet("{numCliente:int}")]
    public async Task<IActionResult> GetByCliente(int numCliente)
    {
        var notas = await _notaCreditoService.GetByClienteAsync(numCliente);
        return Ok(notas);
    }

    [HttpGet("{numCliente:int}/{ncfolio:int}")]
    public async Task<IActionResult> GetByFolio(int numCliente, int ncfolio)
    {
        var nota = await _notaCreditoService.GetByFolioAsync(numCliente, ncfolio);
        return Ok(nota);
    }

    [HttpGet("{numCliente:int}/{ncfolio:int}/pagos")]
    public async Task<IActionResult> GetPagos(int numCliente, int ncfolio)
    {
        var pagos = await _notaCreditoService.GetPagosAsync(numCliente, ncfolio);
        return Ok(pagos);
    }

    [HttpGet("{numCliente:int}/open")]
    public async Task<IActionResult> GetOpen(int numCliente)
    {
        var nota = await _notaCreditoService.GetOpenByClienteAsync(numCliente);
        if (nota is null) return NotFound(new { message = "No hay nota de crédito abierta para este cliente." });
        return Ok(nota);
    }

    [HttpGet("{numCliente:int}/next-folio")]
    public async Task<IActionResult> GetNextFolio(int numCliente)
    {
        var folio = await _notaCreditoService.GetNextFolioAsync(numCliente);
        return Ok(new { nextFolio = folio });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNotaCreditoDto dto)
    {
        var created = await _notaCreditoService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetByFolio), new { numCliente = created.Numcliente, ncfolio = created.Ncfolio }, created);
    }

    [HttpPut("{numCliente:int}/{ncfolio:int}/items")]
    public async Task<IActionResult> AddItems(int numCliente, int ncfolio, [FromBody] AddItemsNotaCreditoDto dto)
    {
        await _notaCreditoService.AddItemsAsync(numCliente, ncfolio, dto);
        return NoContent();
    }

    [HttpPost("{numCliente:int}/{ncfolio:int}/pagos")]
    public async Task<IActionResult> RegistrarPago(int numCliente, int ncfolio, [FromBody] CreatePagoNotaCreditoDto dto)
    {
        var pago = await _notaCreditoService.RegistrarPagoAsync(numCliente, ncfolio, dto);
        return Ok(pago);
    }

    [HttpPatch("{numCliente:int}/{ncfolio:int}/cancelar")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Cancelar(int numCliente, int ncfolio)
    {
        await _notaCreditoService.CancelarAsync(numCliente, ncfolio);
        return NoContent();
    }
}
