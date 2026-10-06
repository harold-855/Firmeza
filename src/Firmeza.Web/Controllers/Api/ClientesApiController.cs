using Firmeza.Application.DTOS.Clientes;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Web.Controllers.Api;

[ApiController]
[Route("api/clientes")]
[Authorize(Roles = Roles.Administrador)]
public class ClientesApiController(IClienteService clienteService, IExportService exportService) : ControllerBase
{
    private readonly IClienteService _clienteService = clienteService;
    private readonly IExportService _exportService = exportService;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] ClienteFilterDto filter)
    {
        var clientes = await _clienteService.GetAllAsync(filter);
        return Ok(clientes);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var cliente = await _clienteService.GetByIdAsync(id);
        if (cliente == null)
            return NotFound(new { message = $"Cliente con ID {id} no fue encontrado." });

        return Ok(cliente);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClienteDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (await _clienteService.DocumentoExistsAsync(dto.DocumentoIdentidad))
        {
            ModelState.AddModelError(nameof(dto.DocumentoIdentidad), "Ya existe un cliente registrado con este documento de identidad o NIT.");
            return BadRequest(ModelState);
        }

        var nuevoCliente = await _clienteService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = nuevoCliente.Id }, nuevoCliente);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClienteDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new { message = "El identificador de la URL no coincide con el modelo." });

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (await _clienteService.DocumentoExistsAsync(dto.DocumentoIdentidad, id))
        {
            ModelState.AddModelError(nameof(dto.DocumentoIdentidad), "Ya existe otro cliente registrado con este documento de identidad o NIT.");
            return BadRequest(ModelState);
        }

        var updated = await _clienteService.UpdateAsync(id, dto);
        if (!updated)
            return NotFound(new { message = $"Cliente con ID {id} no existe." });

        return Ok(new { message = "Cliente actualizado correctamente." });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var exists = await _clienteService.ExistsAsync(id);
        if (!exists)
            return NotFound(new { message = $"Cliente con ID {id} no existe." });

        try
        {
            var deleted = await _clienteService.DeleteAsync(id);
            if (!deleted)
                return StatusCode(500, new { message = "Error al eliminar el cliente." });

            return Ok(new { message = "Cliente eliminado correctamente." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("exportar/excel")]
    public async Task<IActionResult> ExportarExcel([FromQuery] ClienteFilterDto filter, CancellationToken cancellationToken)
    {
        var bytes = await _exportService.ExportarClientesExcelAsync(filter, cancellationToken);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Clientes_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    [HttpGet("exportar/pdf")]
    public async Task<IActionResult> ExportarPdf([FromQuery] ClienteFilterDto filter, CancellationToken cancellationToken)
    {
        var bytes = await _exportService.ExportarClientesPdfAsync(filter, cancellationToken);
        return File(bytes, "application/pdf", $"Clientes_{DateTime.Now:yyyyMMdd}.pdf");
    }
}
