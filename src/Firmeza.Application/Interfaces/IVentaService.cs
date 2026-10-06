using Firmeza.Application.DTOS.Ventas;

namespace Firmeza.Application.Interfaces;

public interface IVentaService
{
    Task<IEnumerable<VentaDto>> GetAllAsync(VentaFilterDto? filter = null, CancellationToken cancellationToken = default);
    Task<VentaDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VentaDto> CreateAsync(CreateVentaDto dto, string? wwwrootPath = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateEstadoDespachoAsync(Guid id, string nuevoEstado, CancellationToken cancellationToken = default);
}
