using Firmeza.Application.DTOS.Clientes;
using Firmeza.Domain.Entities;

namespace Firmeza.Application.Interfaces.Repositories;

public interface IClienteRepository : IBaseRepository<Cliente>
{
    Task<IEnumerable<Cliente>> GetAllWithVentasAsync(ClienteFilterDto? filter = null, CancellationToken cancellationToken = default);
    Task<Cliente?> GetByIdWithVentasAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Cliente?> GetByDocumentoAsync(string documento, CancellationToken cancellationToken = default);
    Task<bool> DocumentoExistsAsync(string documento, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
