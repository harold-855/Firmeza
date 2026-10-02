using Firmeza.Application.Interfaces;
using Firmeza.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Web.Controllers.Api;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = Roles.Administrador)]
public class DashboardApiController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics()
    {
        var metrics = await dashboardService.GetDashboardMetricsAsync();
        return Ok(metrics);
    }
}
