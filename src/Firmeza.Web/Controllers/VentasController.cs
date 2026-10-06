using Firmeza.Application.DTOS.Ventas;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Web.Controllers;

[Authorize(Roles = Roles.Administrador)]
public class VentasController(
    IVentaService ventaService,
    IClienteService clienteService,
    IProductoService productoService,
    IExportService exportService,
    IWebHostEnvironment environment,
    ILogger<VentasController> logger) : Controller
{
    private readonly IVentaService _ventaService = ventaService;
    private readonly IClienteService _clienteService = clienteService;
    private readonly IProductoService _productoService = productoService;
    private readonly IExportService _exportService = exportService;
    private readonly IWebHostEnvironment _environment = environment;
    private readonly ILogger<VentasController> _logger = logger;

    // GET: /Ventas
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] VentaFilterDto filter)
    {
        var ventas = (await _ventaService.GetAllAsync(filter)).ToList();
        var clientes = (await _clienteService.GetAllAsync()).ToList();

        var viewModel = new VentaIndexViewModel
        {
            Ventas = ventas,
            Filter = filter,
            Clientes = clientes,
            TotalVentas = ventas.Count,
            MontoTotalFacturado = ventas.Sum(v => v.Total),
            TotalPendientes = ventas.Count(v => v.EstadoDespacho == "Pendiente"),
            TotalEnRuta = ventas.Count(v => v.EstadoDespacho == "En Ruta"),
            TotalEntregadas = ventas.Count(v => v.EstadoDespacho == "Entregado")
        };

        return View(viewModel);
    }

    // GET: /Ventas/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var venta = await _ventaService.GetByIdAsync(id);
        if (venta == null)
        {
            TempData["Error"] = "La orden de venta solicitada no existe.";
            return RedirectToAction(nameof(Index));
        }

        return View(venta);
    }

    // GET: /Ventas/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var clientes = (await _clienteService.GetAllAsync()).ToList();
        var productos = (await _productoService.GetAllAsync()).Where(p => p.Activo).ToList();

        var viewModel = new VentaCreateViewModel
        {
            Clientes = clientes,
            Productos = productos
        };

        return View(viewModel);
    }

    // POST: /Ventas/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateVentaDto dto)
    {
        if (!ModelState.IsValid || dto.Detalles == null || dto.Detalles.Count == 0)
        {
            TempData["Error"] = "Debe seleccionar un cliente y agregar al menos un producto a la venta.";
            return RedirectToAction(nameof(Create));
        }

        try
        {
            var wwwroot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var ventaCreada = await _ventaService.CreateAsync(dto, wwwroot);

            TempData["Success"] = $"¡Venta {ventaCreada.NumeroComprobante} registrada exitosamente! El recibo PDF ha sido generado y almacenado.";
            return RedirectToAction(nameof(Details), new { id = ventaCreada.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar la venta");
            TempData["Error"] = $"Error al registrar la venta: {ex.Message}";
            return RedirectToAction(nameof(Create));
        }
    }

    // GET: /Ventas/DescargarRecibo/5
    [HttpGet]
    public async Task<IActionResult> DescargarRecibo(Guid id)
    {
        try
        {
            var wwwroot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var recibosDir = Path.Combine(wwwroot, "recibos");
            var filePath = Path.Combine(recibosDir, $"recibo_{id}.pdf");

            byte[] pdfBytes;
            if (System.IO.File.Exists(filePath))
            {
                pdfBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            }
            else
            {
                pdfBytes = await _exportService.GenerarComprobanteReciboPdfAsync(id);
                if (!Directory.Exists(recibosDir)) Directory.CreateDirectory(recibosDir);
                await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);
            }

            return File(pdfBytes, "application/pdf", $"Recibo_Venta_{id.ToString()[..8].ToUpperInvariant()}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al descargar el recibo PDF {VentaId}", id);
            TempData["Error"] = "No se pudo generar o descargar el comprobante en PDF.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: /Ventas/ExportarExcel
    [HttpGet]
    public async Task<IActionResult> ExportarExcel([FromQuery] VentaFilterDto filter)
    {
        try
        {
            var bytes = await _exportService.ExportarVentasExcelAsync(filter);
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Reporte_Ventas_Firmeza_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar ventas a Excel");
            TempData["Error"] = "Ocurrió un error al exportar las ventas a Excel.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: /Ventas/ExportarPdf
    [HttpGet]
    public async Task<IActionResult> ExportarPdf([FromQuery] VentaFilterDto filter)
    {
        try
        {
            var bytes = await _exportService.ExportarVentasPdfAsync(filter);
            return File(
                bytes,
                "application/pdf",
                $"Reporte_Ventas_Firmeza_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar ventas a PDF");
            TempData["Error"] = "Ocurrió un error al exportar las ventas a PDF.";
            return RedirectToAction(nameof(Index));
        }
    }
}

public class VentaIndexViewModel
{
    public List<VentaDto> Ventas { get; set; } = new();
    public VentaFilterDto Filter { get; set; } = new();
    public List<Firmeza.Application.DTOS.Clientes.ClienteDto> Clientes { get; set; } = new();
    public int TotalVentas { get; set; }
    public decimal MontoTotalFacturado { get; set; }
    public int TotalPendientes { get; set; }
    public int TotalEnRuta { get; set; }
    public int TotalEntregadas { get; set; }
}

public class VentaCreateViewModel
{
    public List<Firmeza.Application.DTOS.Clientes.ClienteDto> Clientes { get; set; } = new();
    public List<Firmeza.Application.DTOS.Productos.ProductoDto> Productos { get; set; } = new();
    public CreateVentaDto Venta { get; set; } = new();
}
