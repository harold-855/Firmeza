using System.Security.Claims;
using Firmeza.Application.DTOS.Ventas;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Web.Controllers.Api;

[ApiController]
[Route("api/ventas")]
public class VentasApiController(
    IVentaService ventaService,
    IExportService exportService,
    IWebHostEnvironment environment) : ControllerBase
{
    private readonly IVentaService _ventaService = ventaService;
    private readonly IExportService _exportService = exportService;
    private readonly IWebHostEnvironment _environment = environment;

    [HttpGet]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> GetAll([FromQuery] VentaFilterDto filter, CancellationToken cancellationToken)
    {
        var ventas = await _ventaService.GetAllAsync(filter, cancellationToken);
        return Ok(ventas);
    }

    [HttpGet("mis-pedidos")]
    [Authorize]
    public async Task<IActionResult> GetMisPedidos(CancellationToken cancellationToken)
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid.TryParse(userIdStr, out var userId);

        var ventas = await _ventaService.GetAllAsync(cancellationToken: cancellationToken);
        var misVentas = ventas.Where(v =>
            (userId != Guid.Empty && v.ClienteId == userId) ||
            (!string.IsNullOrWhiteSpace(email) && string.Equals(v.ClienteEmail, email, StringComparison.OrdinalIgnoreCase)) ||
            (!string.IsNullOrWhiteSpace(email) && v.ClienteRazonSocial.Contains(email.Split('@')[0], StringComparison.OrdinalIgnoreCase))
        ).OrderByDescending(v => v.FechaVenta).ToList();

        return Ok(misVentas);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var venta = await _ventaService.GetByIdAsync(id, cancellationToken);
        if (venta == null) return NotFound(new { mensaje = "Venta no encontrada." });
        return Ok(venta);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateVentaDto dto, CancellationToken cancellationToken)
    {
        if (dto.Detalles == null || dto.Detalles.Count == 0)
        {
            return BadRequest(new { mensaje = "La orden de venta debe contener al menos un producto." });
        }

        try
        {
            var wwwroot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var ventaCreada = await _ventaService.CreateAsync(dto, wwwroot, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = ventaCreada.Id }, ventaCreada);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("{id:guid}/recibo")]
    [Authorize]
    public async Task<IActionResult> DescargarRecibo(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var wwwroot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var recibosDir = Path.Combine(wwwroot, "recibos");
            var filePath = Path.Combine(recibosDir, $"recibo_{id}.pdf");

            byte[] pdfBytes;
            if (System.IO.File.Exists(filePath))
            {
                pdfBytes = await System.IO.File.ReadAllBytesAsync(filePath, cancellationToken);
            }
            else
            {
                pdfBytes = await _exportService.GenerarComprobanteReciboPdfAsync(id, cancellationToken);
                if (!Directory.Exists(recibosDir)) Directory.CreateDirectory(recibosDir);
                await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes, cancellationToken);
            }

            return File(pdfBytes, "application/pdf", $"Recibo_Venta_{id.ToString()[..8].ToUpperInvariant()}.pdf");
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { mensaje = "Orden de venta no encontrada." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al generar comprobante PDF.", error = ex.Message });
        }
    }

    [HttpGet("exportar/excel")]
    public async Task<IActionResult> ExportarExcel([FromQuery] VentaFilterDto filter, CancellationToken cancellationToken)
    {
        var bytes = await _exportService.ExportarVentasExcelAsync(filter, cancellationToken);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Ventas_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    [HttpGet("exportar/pdf")]
    public async Task<IActionResult> ExportarPdf([FromQuery] VentaFilterDto filter, CancellationToken cancellationToken)
    {
        var bytes = await _exportService.ExportarVentasPdfAsync(filter, cancellationToken);
        return File(bytes, "application/pdf", $"Ventas_{DateTime.Now:yyyyMMdd}.pdf");
    }
}
