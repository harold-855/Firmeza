using Firmeza.Domain.Constants;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Web.Controllers.Api;

[ApiController]
[Route("api/ventas")]
[Authorize(Roles = Roles.Administrador)]
public class VentasApiController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var ventas = await context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
            .ThenInclude(d => d.Producto)
            .OrderByDescending(v => v.FechaVenta)
            .Select(v => new
            {
                v.Id,
                Cliente = v.Cliente != null ? v.Cliente.RazonSocial : "Cliente General",
                DocumentoCliente = v.Cliente != null ? v.Cliente.DocumentoIdentidad : "",
                Fecha = v.FechaVenta,
                v.Total,
                v.EstadoDespacho,
                TotalItems = v.Detalles.Count
            })
            .ToListAsync();

        return Ok(ventas);
    }
}
