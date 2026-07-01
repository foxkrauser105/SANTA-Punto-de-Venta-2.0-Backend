using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SANTA.PoS.Business.DTOs;
using SANTA.PoS.Business.Services;

namespace SANTA.PoS.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProductsController(ProductService productService) : ControllerBase
{
    private readonly ProductService _productService = productService;

    [HttpGet]
    public async Task<IActionResult> GetAllProducts()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(string id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        return Ok(product);
    }

    [HttpGet("filter")]
    public async Task<IActionResult> GetProductsByFilter([FromQuery] string filter)
    {
        var products = await _productService.GetFilteredProductsAsync(filter);
        return Ok(products);
    }

    [HttpGet("discount")]
    public async Task<IActionResult> GetProductsWithDiscount()
    {
        var products = await _productService.GetProductsWithDiscountAsync();
        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateUpdateProductDto product)
    {
        var createdProduct = await _productService.CreateProductAsync(product);
        return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.IdProducto }, createdProduct);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(string id, [FromBody] CreateUpdateProductDto product)
    {
        await _productService.UpdateProductDataAsync(id, product);
        return NoContent();
    }

    [HttpPut("Status/{id}")]
    public async Task<IActionResult> UpdateProductStatus(string id, [FromBody] CreateUpdateProductDto product)
    {
        await _productService.UpdateProductStatusAsync(id, product);
        return NoContent();
    }

    [HttpPut("Descuento/{id}")]
    public async Task<IActionResult> UpdateProductDiscount(string id, [FromBody] CreateUpdateProductDto product)
    {
        await _productService.UpdateProductDiscountAsync(id, product);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        await _productService.DeleteProductAsync(id);
        return NoContent();
    }

    [HttpGet("{id}/precio")]
    public async Task<IActionResult> GetPrecioParaVenta(string id, [FromQuery] double cantidad = 0)
    {
        var precio = await _productService.GetPrecioParaVentaAsync(id, cantidad);
        return Ok(precio);
    }

    [HttpPatch("{id}/codigo")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RenameCodigo(string id, [FromBody] RenameProductoDto dto)
    {
        await _productService.RenameCodigoAsync(id, dto.NuevoCodigo);
        return NoContent();
    }
}
