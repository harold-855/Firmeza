using Firmeza.Application.DTOS.Productos;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Web.Controllers.Api;

[ApiController]
[Route("api/productos")]
[Authorize(Roles = Roles.Administrador)]
public class ProductosApiController(IProductoService productoService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous] // Permitir consulta desde SPA / cliente
    public async Task<IActionResult> GetAll([FromQuery] ProductoFilterDto filter)
    {
        var productos = await productoService.GetAllAsync(filter);
        return Ok(productos);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var producto = await productoService.GetByIdAsync(id);
        if (producto == null)
            return NotFound(new { message = $"Producto con ID {id} no fue encontrado." });

        return Ok(producto);
    }

    [HttpGet("unidades-medida")]
    [AllowAnonymous]
    public async Task<IActionResult> GetUnidadesMedida()
    {
        var unidades = await productoService.GetUnidadesMedidaAsync();
        return Ok(unidades);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductoDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var nuevoProducto = await productoService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = nuevoProducto.Id }, nuevoProducto);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductoDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new { message = "El identificador de la URL no coincide con el modelo." });

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await productoService.UpdateAsync(id, dto);
        if (!updated)
            return NotFound(new { message = $"Producto con ID {id} no existe." });

        return Ok(new { message = "Producto actualizado correctamente." });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var exists = await productoService.ExistsAsync(id);
        if (!exists)
            return NotFound(new { message = $"Producto con ID {id} no existe." });

        var deleted = await productoService.DeleteAsync(id);
        if (!deleted)
            return StatusCode(500, new { message = "Error al eliminar o desactivar el producto." });

        return Ok(new { message = "Producto eliminado o desactivado correctamente." });
    }
}
