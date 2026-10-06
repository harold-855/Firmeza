using Firmeza.Application.DTOS.Clientes;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Constants;
using Firmeza.Web.Models.Clientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Web.Controllers;

[Authorize(Roles = Roles.Administrador)]
public class ClientesController(IClienteService clienteService, IExportService exportService) : Controller
{
    // GET: /Clientes
    public async Task<IActionResult> Index([FromQuery] ClienteFilterDto filter)
    {
        var clientes = (await clienteService.GetAllAsync(filter)).ToList();
        var todos = (await clienteService.GetAllAsync()).ToList();

        var viewModel = new ClienteIndexViewModel
        {
            Clientes = clientes,
            Filter = filter,
            TotalClientes = todos.Count,
            ClientesConCompras = todos.Count(c => c.TotalCompras > 0),
            MontoTotalAcumulado = todos.Sum(c => c.MontoTotalComprado)
        };

        return View(viewModel);
    }

    // GET: /Clientes/Details/5
    public async Task<IActionResult> Details(Guid id)
    {
        var cliente = await clienteService.GetByIdAsync(id);
        if (cliente == null)
        {
            TempData["Error"] = "El cliente solicitado no existe.";
            return RedirectToAction(nameof(Index));
        }

        return View(cliente);
    }

    // GET: /Clientes/Create
    public IActionResult Create()
    {
        return View(new CreateClienteDto());
    }

    // POST: /Clientes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateClienteDto model)
    {
        if (await clienteService.DocumentoExistsAsync(model.DocumentoIdentidad))
        {
            ModelState.AddModelError(nameof(model.DocumentoIdentidad), "Ya existe un cliente registrado con este número de Documento o NIT.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var created = await clienteService.CreateAsync(model);
            TempData["Success"] = $"Cliente \"{created.RazonSocial}\" registrado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error al registrar el cliente: {ex.Message}");
            return View(model);
        }
    }

    // GET: /Clientes/Edit/5
    public async Task<IActionResult> Edit(Guid id)
    {
        var cliente = await clienteService.GetByIdAsync(id);
        if (cliente == null)
        {
            TempData["Error"] = "El cliente que intentas editar no existe.";
            return RedirectToAction(nameof(Index));
        }

        var updateDto = new UpdateClienteDto
        {
            Id = cliente.Id,
            DocumentoIdentidad = cliente.DocumentoIdentidad,
            RazonSocial = cliente.RazonSocial,
            Telefono = cliente.Telefono,
            DireccionEnvio = cliente.DireccionEnvio,
            Email = cliente.Email
        };

        return View(updateDto);
    }

    // POST: /Clientes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateClienteDto model)
    {
        if (id != model.Id)
        {
            TempData["Error"] = "Inconsistencia en los datos del cliente.";
            return RedirectToAction(nameof(Index));
        }

        if (await clienteService.DocumentoExistsAsync(model.DocumentoIdentidad, id))
        {
            ModelState.AddModelError(nameof(model.DocumentoIdentidad), "Ya existe otro cliente registrado con este número de Documento o NIT.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var updated = await clienteService.UpdateAsync(id, model);
            if (!updated)
            {
                TempData["Error"] = "No se pudo actualizar el cliente.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = $"Cliente \"{model.RazonSocial}\" actualizado con éxito.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error al actualizar el cliente: {ex.Message}");
            return View(model);
        }
    }

    // GET: /Clientes/Delete/5
    public async Task<IActionResult> Delete(Guid id)
    {
        var cliente = await clienteService.GetByIdAsync(id);
        if (cliente == null)
        {
            TempData["Error"] = "El cliente no existe.";
            return RedirectToAction(nameof(Index));
        }

        return View(cliente);
    }

    // POST: /Clientes/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var cliente = await clienteService.GetByIdAsync(id);
        if (cliente == null)
        {
            TempData["Error"] = "El cliente ya no existe.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var deleted = await clienteService.DeleteAsync(id);
            if (deleted)
            {
                TempData["Success"] = $"El cliente \"{cliente.RazonSocial}\" fue eliminado exitosamente.";
            }
            else
            {
                TempData["Error"] = "No fue posible eliminar el cliente.";
            }
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al eliminar el cliente: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: /Clientes/ExportarExcel
    [HttpGet]
    public async Task<IActionResult> ExportarExcel([FromQuery] ClienteFilterDto filter)
    {
        try
        {
            var bytes = await exportService.ExportarClientesExcelAsync(filter);
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Directorio_Clientes_Firmeza_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }
        catch (Exception)
        {
            TempData["Error"] = "Error al exportar los clientes a Excel.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: /Clientes/ExportarPdf
    [HttpGet]
    public async Task<IActionResult> ExportarPdf([FromQuery] ClienteFilterDto filter)
    {
        try
        {
            var bytes = await exportService.ExportarClientesPdfAsync(filter);
            return File(
                bytes,
                "application/pdf",
                $"Directorio_Clientes_Firmeza_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }
        catch (Exception)
        {
            TempData["Error"] = "Error al exportar los clientes a PDF.";
            return RedirectToAction(nameof(Index));
        }
    }
}
