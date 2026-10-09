using System.Security.Claims;
using Firmeza.Application.DTOS.Ventas;
using Firmeza.Application.Interfaces;
using Firmeza.Application.UseCases.Ventas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Api.Controllers;

[ApiController]
[Route("api/ventas")]
public class VentasApiController(
    ObtenerVentasUseCase obtenerVentasUseCase,
    CrearVentaUseCase crearVentaUseCase,
    ActualizarEstadoDespachoUseCase actualizarEstadoDespachoUseCase,
    IExportService exportService,
    IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> GetAll([FromQuery] VentaFilterDto filter, CancellationToken cancellationToken)
    {
        var ventas = await obtenerVentasUseCase.ExecuteGetAllAsync(filter, cancellationToken);
        return Ok(ventas);
    }

    [HttpGet("mis-pedidos")]
    [Authorize(Policy = "RequireAnyRole")]
    public async Task<IActionResult> GetMisPedidos(CancellationToken cancellationToken)
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid.TryParse(userIdStr, out var userId);

        var ventas = await obtenerVentasUseCase.ExecuteGetAllAsync(cancellationToken: cancellationToken);
        var misVentas = ventas.Where(v =>
            (userId != Guid.Empty && v.ClienteId == userId) ||
            (!string.IsNullOrWhiteSpace(email) && string.Equals(v.ClienteEmail, email, StringComparison.OrdinalIgnoreCase)) ||
            (!string.IsNullOrWhiteSpace(email) && v.ClienteRazonSocial.Contains(email.Split('@')[0], StringComparison.OrdinalIgnoreCase))
        ).OrderByDescending(v => v.FechaVenta).ToList();

        return Ok(misVentas);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "RequireAnyRole")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var venta = await obtenerVentasUseCase.ExecuteGetByIdAsync(id, cancellationToken);
        if (venta == null)
            return NotFound(new { mensaje = "La orden de venta solicitada no existe." });

        return Ok(venta);
    }

    [HttpPost]
    [Authorize(Policy = "RequireAnyRole")]
    public async Task<IActionResult> Create([FromBody] CreateVentaDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            if (string.IsNullOrWhiteSpace(dto.ClienteEmail))
            {
                dto.ClienteEmail = User.FindFirstValue(ClaimTypes.Email);
            }

            var rutaWwwroot = environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var creada = await crearVentaUseCase.ExecuteAsync(dto, rutaWwwroot, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = creada.Id }, creada);
        }
        catch (Exception ex) when (ex is ArgumentException or KeyNotFoundException or InvalidOperationException)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/estado")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> UpdateEstado(Guid id, [FromBody] ActualizarEstadoDespachoDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.EstadoDespacho))
            return BadRequest(new { mensaje = "El nuevo estado de despacho es obligatorio." });

        var actualizado = await actualizarEstadoDespachoUseCase.ExecuteAsync(id, dto.EstadoDespacho, cancellationToken);
        if (!actualizado)
            return NotFound(new { mensaje = "No se encontró la venta indicada." });

        return NoContent();
    }

    [HttpGet("{id:guid}/recibo")]
    [Authorize(Policy = "RequireAnyRole")]
    public async Task<IActionResult> DescargarRecibo(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var pdfBytes = await exportService.GenerarComprobanteReciboPdfAsync(id, cancellationToken);
            return File(pdfBytes, "application/pdf", $"recibo_venta_{id}.pdf");
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { mensaje = "La orden de venta no fue encontrada." });
        }
    }
}
