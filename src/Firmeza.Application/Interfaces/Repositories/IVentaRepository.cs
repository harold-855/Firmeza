using Firmeza.Domain.Entities;

namespace Firmeza.Application.Interfaces.Repositories;

public interface IVentaRepository : IBaseRepository<Venta>
{
    Task<IEnumerable<Venta>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);
    Task<Venta?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Venta>> GetUltimasVentasAsync(int count, CancellationToken cancellationToken = default);
}
