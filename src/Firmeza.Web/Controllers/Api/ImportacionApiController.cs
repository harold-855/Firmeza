using Firmeza.Application.DTOS.Importacion;
using Firmeza.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Web.Controllers.Api;

[ApiController]
[Route("api/importacion")]
public class ImportacionApiController(IExcelImportService excelImportService, ILogger<ImportacionApiController> logger) : ControllerBase
{
    private readonly IExcelImportService _excelImportService = excelImportService;
    private readonly ILogger<ImportacionApiController> _logger = logger;

    /// <summary>
    /// Procesa e importa un archivo Excel (.xlsx) con datos mezclados/no normalizados.
    /// </summary>
    [HttpPost("excel")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ExcelImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportarExcel(
        IFormFile? file,
        [FromForm] bool actualizarSiExiste = true,
        [FromForm] bool crearVentasSiHayDatos = true,
        [FromForm] bool descontarStockDeVentas = true,
        [FromForm] string? nombreHoja = null,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { mensaje = "Debe adjuntar un archivo de Excel (.xlsx)." });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".xlsx" && extension != ".xls")
        {
            return BadRequest(new { mensaje = "Solo se admiten archivos en formato Excel (.xlsx, .xls)." });
        }

        var options = new ExcelImportOptionsDto
        {
            ActualizarSiExiste = actualizarSiExiste,
            CrearVentasSiHayDatos = crearVentasSiHayDatos,
            DescontarStockDeVentas = descontarStockDeVentas,
            NombreHoja = nombreHoja
        };

        try
        {
            using var stream = file.OpenReadStream();
            var resultado = await _excelImportService.ImportarExcelDesorganizadoAsync(stream, options, cancellationToken);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ImportacionApiController");
            return StatusCode(500, new { mensaje = "Error interno durante la importación", error = ex.Message });
        }
    }

    /// <summary>
    /// Descarga la plantilla de Excel (.xlsx) con ejemplos de datos desorganizados y mixtos.
    /// </summary>
    [HttpGet("plantilla")]
    public IActionResult DescargarPlantilla()
    {
        try
        {
            var bytes = _excelImportService.GenerarPlantillaEjemplo();
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Plantilla_Importacion_{DateTime.Now:yyyyMMdd}.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar plantilla");
            return StatusCode(500, new { mensaje = "No se pudo generar la plantilla." });
        }
    }
}
