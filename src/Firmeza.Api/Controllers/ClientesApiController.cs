using Firmeza.Application.DTOS.Clientes;
using Firmeza.Application.UseCases.Clientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Api.Controllers;

[ApiController]
[Route("api/clientes")]
[Authorize(Policy = "RequireAdminRole")]
public class ClientesApiController(
    ObtenerClientesUseCase obtenerClientesUseCase,
    CrearClienteUseCase crearClienteUseCase,
    ActualizarClienteUseCase actualizarClienteUseCase,
    EliminarClienteUseCase eliminarClienteUseCase) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ClienteFilterDto filter, CancellationToken cancellationToken)
    {
        var clientes = await obtenerClientesUseCase.ExecuteGetAllAsync(filter, cancellationToken);
        return Ok(clientes);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var cliente = await obtenerClientesUseCase.ExecuteGetByIdAsync(id, cancellationToken);
        if (cliente == null)
            return NotFound(new { mensaje = "El cliente solicitado no fue encontrado." });

        return Ok(cliente);
    }

    [HttpGet("documento/{documento}")]
    public async Task<IActionResult> GetByDocumento(string documento, CancellationToken cancellationToken)
    {
        var cliente = await obtenerClientesUseCase.ExecuteGetByDocumentoAsync(documento, cancellationToken);
        if (cliente == null)
            return NotFound(new { mensaje = "No se encontró cliente con el documento indicado." });

        return Ok(cliente);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClienteDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var yaExiste = await obtenerClientesUseCase.ExecuteDocumentoExistsAsync(dto.DocumentoIdentidad, null, cancellationToken);
        if (yaExiste)
        {
            return Conflict(new { mensaje = $"Ya existe un cliente con el documento {dto.DocumentoIdentidad}." });
        }

        var creado = await crearClienteUseCase.ExecuteAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClienteDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
            return BadRequest(new { mensaje = "Inconsistencia de ID en la petición." });

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var yaExiste = await obtenerClientesUseCase.ExecuteDocumentoExistsAsync(dto.DocumentoIdentidad, id, cancellationToken);
        if (yaExiste)
        {
            return Conflict(new { mensaje = $"El documento {dto.DocumentoIdentidad} ya está registrado para otro cliente." });
        }

        var actualizado = await actualizarClienteUseCase.ExecuteAsync(id, dto, cancellationToken);
        if (!actualizado)
            return NotFound(new { mensaje = "Cliente no encontrado." });

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var eliminado = await eliminarClienteUseCase.ExecuteAsync(id, cancellationToken);
            if (!eliminado)
                return NotFound(new { mensaje = "Cliente no encontrado." });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }
}
