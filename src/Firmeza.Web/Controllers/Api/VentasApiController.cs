using Firmeza.Application.Interfaces.Repositories;
using Firmeza.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Web.Controllers.Api;

[ApiController]
[Route("api/ventas")]
[Authorize(Roles = Roles.Administrador)]
public class VentasApiController(IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var ventas = await unitOfWork.Ventas.GetAllWithDetailsAsync();

        var result = ventas.Select(v => new
        {
            v.Id,
            Cliente = v.Cliente != null ? v.Cliente.RazonSocial : "Cliente General",
            DocumentoCliente = v.Cliente != null ? v.Cliente.DocumentoIdentidad : "",
            Fecha = v.FechaVenta,
            v.Total,
            v.EstadoDespacho,
            TotalItems = v.Detalles.Count
        });

        return Ok(result);
    }
}
