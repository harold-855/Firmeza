using Firmeza.Application.DTOS.Productos;
using Firmeza.Domain.Entities;

namespace Firmeza.Application.Interfaces.Repositories;

public interface IProductoRepository : IBaseRepository<Producto>
{
    Task<IEnumerable<Producto>> GetAllWithDetallesAsync(ProductoFilterDto? filter = null, CancellationToken cancellationToken = default);
    Task<Producto?> GetByIdWithDetallesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetUnidadesMedidaAsync(CancellationToken cancellationToken = default);
}
