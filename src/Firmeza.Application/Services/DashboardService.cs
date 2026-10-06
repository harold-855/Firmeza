using Firmeza.Application.DTOS.Dashboard;
using Firmeza.Application.Interfaces;
using Firmeza.Application.Interfaces.Repositories;
using Firmeza.Application.UseCases.Dashboard;

namespace Firmeza.Application.Services;

public class DashboardService(ObtenerDashboardMetricsUseCase obtenerDashboardMetricsUseCase) : IDashboardService
{
    public DashboardService(IUnitOfWork unitOfWork)
        : this(new ObtenerDashboardMetricsUseCase(unitOfWork))
    {
    }

    public Task<DashboardMetricsDto> GetDashboardMetricsAsync()
        => obtenerDashboardMetricsUseCase.ExecuteAsync();
}
