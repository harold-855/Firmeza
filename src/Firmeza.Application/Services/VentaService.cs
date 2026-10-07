using Firmeza.Application.DTOS.Ventas;
using Firmeza.Application.Interfaces;
using Firmeza.Application.UseCases.Ventas;

namespace Firmeza.Application.Services;

public class VentaService(
    ObtenerVentasUseCase obtenerVentasUseCase,
    CrearVentaUseCase crearVentaUseCase,
    ActualizarEstadoDespachoUseCase actualizarEstadoDespachoUseCase) : IVentaService
{
    public Task<IEnumerable<VentaDto>> GetAllAsync(VentaFilterDto? filter = null, CancellationToken cancellationToken = default)
        => obtenerVentasUseCase.ExecuteGetAllAsync(filter, cancellationToken);

    public Task<VentaDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => obtenerVentasUseCase.ExecuteGetByIdAsync(id, cancellationToken);

    public Task<VentaDto> CreateAsync(CreateVentaDto dto, string? wwwrootPath = null, CancellationToken cancellationToken = default)
        => crearVentaUseCase.ExecuteAsync(dto, wwwrootPath, cancellationToken);

    public Task<bool> UpdateEstadoDespachoAsync(Guid id, string nuevoEstado, CancellationToken cancellationToken = default)
        => actualizarEstadoDespachoUseCase.ExecuteAsync(id, nuevoEstado, cancellationToken);
}
