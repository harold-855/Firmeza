using Firmeza.Application.DTOS.Productos;
using Firmeza.Application.UseCases.Productos;
using Firmeza.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Api.Controllers;

[ApiController]
[Route("api/productos")]
public class ProductosApiController(
    ObtenerProductosUseCase obtenerProductosUseCase,
    CrearProductoUseCase crearProductoUseCase,
    ActualizarProductoUseCase actualizarProductoUseCase,
    EliminarProductoUseCase eliminarProductoUseCase) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] ProductoFilterDto filter, CancellationToken cancellationToken)
    {
        var productos = await obtenerProductosUseCase.ExecuteGetAllAsync(filter, cancellationToken);
        return Ok(productos);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var producto = await obtenerProductosUseCase.ExecuteGetByIdAsync(id, cancellationToken);
        if (producto == null)
            return NotFound(new { mensaje = "El producto solicitado no existe." });

        return Ok(producto);
    }

    [HttpPost]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> Create([FromBody] CreateProductoDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var creado = await crearProductoUseCase.ExecuteAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductoDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
            return BadRequest(new { mensaje = "El ID de la ruta no coincide con el cuerpo." });

        var actualizado = await actualizarProductoUseCase.ExecuteAsync(id, dto, cancellationToken);
        if (!actualizado)
            return NotFound(new { mensaje = "No se encontró el producto a actualizar." });

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var eliminado = await eliminarProductoUseCase.ExecuteAsync(id, cancellationToken);
        if (!eliminado)
            return NotFound(new { mensaje = "El producto no existe o no se pudo procesar la eliminación." });

        return NoContent();
    }
}
