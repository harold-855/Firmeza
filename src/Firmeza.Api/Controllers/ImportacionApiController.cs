using Firmeza.Application.DTOS.Importacion;
using Firmeza.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Api.Controllers;

[ApiController]
[Route("api/importacion")]
[Authorize(Policy = "RequireAdminRole")]
public class ImportacionApiController(IExcelImportService excelImportService) : ControllerBase
{
    [HttpPost("excel")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ImportarExcel(
        IFormFile? archivo,
        [FromForm] bool actualizarExistentes = true,
        [FromForm] bool crearVentas = true,
        [FromForm] bool descontarInventario = true,
        CancellationToken cancellationToken = default)
    {
        if (archivo == null || archivo.Length == 0)
        {
            return BadRequest(new { mensaje = "Por favor seleccione un archivo Excel válido (.xlsx o .xls)." });
        }

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (extension is not ".xlsx" and not ".xls")
        {
            return BadRequest(new { mensaje = "El formato de archivo no es soportado. Solo se permiten hojas .xlsx o .xls." });
        }

        var options = new ExcelImportOptionsDto
        {
            ActualizarSiExiste = actualizarExistentes,
            CrearVentasSiHayDatos = crearVentas,
            DescontarStockDeVentas = descontarInventario
        };

        using var stream = archivo.OpenReadStream();
        var resultado = await excelImportService.ImportarExcelDesorganizadoAsync(stream, options, cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("plantilla")]
    [AllowAnonymous]
    public IActionResult DescargarPlantilla()
    {
        var bytes = excelImportService.GenerarPlantillaEjemplo();
        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Plantilla_Importacion_Firmeza_Ejemplo.xlsx");
    }
}
