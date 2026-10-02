using Firmeza.Application.DTOS.Dashboard;

namespace Firmeza.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardMetricsDto> GetDashboardMetricsAsync();
}
