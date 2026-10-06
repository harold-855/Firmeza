using Firmeza.Application.DTOS.Productos;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Constants;
using Firmeza.Web.Models.Productos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Web.Controllers;

[Authorize(Roles = Roles.Administrador)]
public class ProductosController(IProductoService productoService, IExportService exportService) : Controller
{
    // GET: /Productos
    public async Task<IActionResult> Index([FromQuery] ProductoFilterDto filter)
    {
        var productos = (await productoService.GetAllAsync(filter)).ToList();
        var unidades = await productoService.GetUnidadesMedidaAsync();

        // Métricas rápidas para las tarjetas informativas
        var todos = (await productoService.GetAllAsync()).ToList();

        var viewModel = new ProductoIndexViewModel
        {
            Productos = productos,
            Filter = filter,
            UnidadesMedida = unidades,
            TotalProductos = todos.Count,
            TotalActivos = todos.Count(p => p.Activo),
            TotalBajoStock = todos.Count(p => p.StockActual < 50)
        };

        return View(viewModel);
    }

    // GET: /Productos/Details/5
    public async Task<IActionResult> Details(Guid id)
    {
        var producto = await productoService.GetByIdAsync(id);
        if (producto == null)
        {
            TempData["Error"] = "El producto solicitado no existe.";
            return RedirectToAction(nameof(Index));
        }

        return View(producto);
    }

    // GET: /Productos/Create
    public IActionResult Create()
    {
        var model = new CreateProductoDto
        {
            Activo = true,
            StockActual = 0,
            PrecioUnitario = 0
        };
        return View(model);
    }

    // POST: /Productos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductoDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var created = await productoService.CreateAsync(model);
            TempData["Success"] = $"Producto \"{created.Nombre}\" creado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error al registrar el producto: {ex.Message}");
            return View(model);
        }
    }

    // GET: /Productos/Edit/5
    public async Task<IActionResult> Edit(Guid id)
    {
        var producto = await productoService.GetByIdAsync(id);
        if (producto == null)
        {
            TempData["Error"] = "El producto que intentas editar no existe.";
            return RedirectToAction(nameof(Index));
        }

        var updateDto = new UpdateProductoDto
        {
            Id = producto.Id,
            Nombre = producto.Nombre,
            Descripcion = producto.Descripcion,
            UnidadMedida = producto.UnidadMedida,
            PrecioUnitario = producto.PrecioUnitario,
            StockActual = producto.StockActual,
            Activo = producto.Activo
        };

        return View(updateDto);
    }

    // POST: /Productos/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateProductoDto model)
    {
        if (id != model.Id)
        {
            TempData["Error"] = "Inconsistencia en los datos del producto.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var updated = await productoService.UpdateAsync(id, model);
            if (!updated)
            {
                TempData["Error"] = "No se pudo actualizar el producto.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = $"Producto \"{model.Nombre}\" actualizado con éxito.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error al actualizar el producto: {ex.Message}");
            return View(model);
        }
    }

    // GET: /Productos/Delete/5
    public async Task<IActionResult> Delete(Guid id)
    {
        var producto = await productoService.GetByIdAsync(id);
        if (producto == null)
        {
            TempData["Error"] = "El producto no existe.";
            return RedirectToAction(nameof(Index));
        }

        return View(producto);
    }

    // POST: /Productos/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var producto = await productoService.GetByIdAsync(id);
        if (producto == null)
        {
            TempData["Error"] = "El producto ya no existe.";
            return RedirectToAction(nameof(Index));
        }

        var deleted = await productoService.DeleteAsync(id);
        if (deleted)
        {
            if (producto.TotalVentasAsociadas > 0)
            {
                TempData["Success"] = $"El producto \"{producto.Nombre}\" tiene ventas registradas, por lo que fue desactivado del catálogo para preservar el historial.";
            }
            else
            {
                TempData["Success"] = $"El producto \"{producto.Nombre}\" fue eliminado definitivamente.";
            }
        }
        else
        {
            TempData["Error"] = "No fue posible eliminar el producto.";
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: /Productos/ExportarExcel
    [HttpGet]
    public async Task<IActionResult> ExportarExcel([FromQuery] ProductoFilterDto filter)
    {
        try
        {
            var bytes = await exportService.ExportarProductosExcelAsync(filter);
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Catalogo_Productos_Firmeza_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }
        catch (Exception)
        {
            TempData["Error"] = "Error al exportar los productos a Excel.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: /Productos/ExportarPdf
    [HttpGet]
    public async Task<IActionResult> ExportarPdf([FromQuery] ProductoFilterDto filter)
    {
        try
        {
            var bytes = await exportService.ExportarProductosPdfAsync(filter);
            return File(
                bytes,
                "application/pdf",
                $"Catalogo_Productos_Firmeza_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }
        catch (Exception)
        {
            TempData["Error"] = "Error al exportar los productos a PDF.";
            return RedirectToAction(nameof(Index));
        }
    }
}
