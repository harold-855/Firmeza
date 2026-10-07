using Firmeza.Application.UseCases.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Policy = "RequireAdminRole")]
public class DashboardApiController(ObtenerDashboardMetricsUseCase dashboardMetricsUseCase) : ControllerBase
{
    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics(CancellationToken cancellationToken)
    {
        var metrics = await dashboardMetricsUseCase.ExecuteAsync(cancellationToken);
        return Ok(metrics);
    }
}
