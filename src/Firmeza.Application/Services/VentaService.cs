using Firmeza.Application.DTOS.Ventas;
using Firmeza.Application.Interfaces;
using Firmeza.Application.Interfaces.Repositories;
using Firmeza.Application.UseCases.Ventas;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Firmeza.Application.Services;

public class VentaService(
    ObtenerVentasUseCase obtenerVentasUseCase,
    CrearVentaUseCase crearVentaUseCase,
    ActualizarEstadoDespachoUseCase actualizarEstadoDespachoUseCase) : IVentaService
{
    public VentaService(IUnitOfWork unitOfWork, IExportService exportService, ILogger<VentaService>? logger = null)
        : this(
            new ObtenerVentasUseCase(unitOfWork),
            new CrearVentaUseCase(unitOfWork, exportService, NullLogger<CrearVentaUseCase>.Instance),
            new ActualizarEstadoDespachoUseCase(unitOfWork))
    {
    }

    public VentaService(IUnitOfWork unitOfWork, IExportService exportService, ILogger<CrearVentaUseCase>? logger = null)
        : this(
            new ObtenerVentasUseCase(unitOfWork),
            new CrearVentaUseCase(unitOfWork, exportService, logger ?? NullLogger<CrearVentaUseCase>.Instance),
            new ActualizarEstadoDespachoUseCase(unitOfWork))
    {
    }

    public Task<IEnumerable<VentaDto>> GetAllAsync(VentaFilterDto? filter = null, CancellationToken cancellationToken = default)
        => obtenerVentasUseCase.ExecuteGetAllAsync(filter, cancellationToken);

    public Task<VentaDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => obtenerVentasUseCase.ExecuteGetByIdAsync(id, cancellationToken);

    public Task<VentaDto> CreateAsync(CreateVentaDto dto, string? wwwrootPath = null, CancellationToken cancellationToken = default)
        => crearVentaUseCase.ExecuteAsync(dto, wwwrootPath, cancellationToken);

    public Task<bool> UpdateEstadoDespachoAsync(Guid id, string nuevoEstado, CancellationToken cancellationToken = default)
        => actualizarEstadoDespachoUseCase.ExecuteAsync(id, nuevoEstado, cancellationToken);
}
