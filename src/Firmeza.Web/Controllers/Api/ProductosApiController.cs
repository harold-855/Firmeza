using Firmeza.Domain.Constants;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Web.Controllers.Api;

[ApiController]
[Route("api/productos")]
[Authorize(Roles = Roles.Administrador)]
public class ProductosApiController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var productos = await context.Productos
            .OrderBy(p => p.Nombre)
            .Select(p => new
            {
                p.Id,
                p.Nombre,
                p.Descripcion,
                p.UnidadMedida,
                p.PrecioUnitario,
                p.StockActual,
                p.Activo
            })
            .ToListAsync();

        return Ok(productos);
    }
}
