using Firmeza.Application.DTOS.Importacion;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Web.Controllers;

[Authorize(Roles = Roles.Administrador)]
public class ImportacionController(IExcelImportService excelImportService, ILogger<ImportacionController> logger) : Controller
{
    private readonly IExcelImportService _excelImportService = excelImportService;
    private readonly ILogger<ImportacionController> _logger = logger;

    // GET: /Importacion
    [HttpGet]
    public IActionResult Index()
    {
        return View(new ExcelImportViewModel());
    }

    // POST: /Importacion/Procesar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Procesar(
        IFormFile? archivoExcel,
        ExcelImportOptionsDto options,
        CancellationToken cancellationToken)
    {
        var model = new ExcelImportViewModel { Options = options };

        if (archivoExcel == null || archivoExcel.Length == 0)
        {
            ModelState.AddModelError("ArchivoExcel", "Por favor seleccione un archivo .xlsx para importar.");
            return View("Index", model);
        }

        var extension = Path.GetExtension(archivoExcel.FileName).ToLowerInvariant();
        if (extension != ".xlsx" && extension != ".xls")
        {
            ModelState.AddModelError("ArchivoExcel", "Formato de archivo no compatible. Solo se admiten archivos de Excel (.xlsx, .xls).");
            return View("Index", model);
        }

        try
        {
            using var stream = archivoExcel.OpenReadStream();
            var resultado = await _excelImportService.ImportarExcelDesorganizadoAsync(stream, options, cancellationToken);
            model.Resultado = resultado;
            model.NombreArchivo = archivoExcel.FileName;

            if (resultado.Exitoso)
            {
                TempData["Success"] = $"¡Importación procesada! {resultado.ClientesCreados + resultado.ClientesActualizados} clientes, {resultado.ProductosCreados + resultado.ProductosActualizados} productos y {resultado.VentasCreadas} ventas procesadas.";
            }
            else
            {
                TempData["Warning"] = $"La importación finalizó con {resultado.TotalErrores} errores y {resultado.TotalAdvertencias} advertencias. Revise el detalle a continuación.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar archivo Excel no normalizado");
            TempData["Error"] = $"Ocurrió un error inesperado al procesar el archivo: {ex.Message}";
        }

        return View("Index", model);
    }

    // GET: /Importacion/DescargarPlantilla
    [HttpGet]
    public IActionResult DescargarPlantilla()
    {
        try
        {
            var bytes = _excelImportService.GenerarPlantillaEjemplo();
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Plantilla_Importacion_Firmeza_{DateTime.Now:yyyyMMdd}.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar la plantilla de Excel");
            TempData["Error"] = "No se pudo generar la plantilla de ejemplo.";
            return RedirectToAction(nameof(Index));
        }
    }
}

public class ExcelImportViewModel
{
    public ExcelImportOptionsDto Options { get; set; } = new();
    public ExcelImportResultDto? Resultado { get; set; }
    public string? NombreArchivo { get; set; }
}
