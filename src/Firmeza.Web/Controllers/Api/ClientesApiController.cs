using Firmeza.Domain.Constants;
using Firmeza.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Web.Controllers.Api;

[ApiController]
[Route("api/clientes")]
[Authorize(Roles = Roles.Administrador)]
public class ClientesApiController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var clientes = await context.Clientes
            .OrderBy(c => c.RazonSocial)
            .Select(c => new
            {
                c.Id,
                c.RazonSocial,
                c.DocumentoIdentidad,
                c.Telefono,
                c.Email,
                c.DireccionEnvio,
                TotalCompras = c.Ventas.Count
            })
            .ToListAsync();

        return Ok(clientes);
    }
}
