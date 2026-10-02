using Firmeza.Application.DTOS.Clientes;

namespace Firmeza.Application.Interfaces;

public interface IClienteService
{
    Task<IEnumerable<ClienteDto>> GetAllAsync(ClienteFilterDto? filter = null);
    Task<ClienteDto?> GetByIdAsync(Guid id);
    Task<ClienteDto?> GetByDocumentoAsync(string documento);
    Task<ClienteDto> CreateAsync(CreateClienteDto dto);
    Task<bool> UpdateAsync(Guid id, UpdateClienteDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> DocumentoExistsAsync(string documento, Guid? excludeId = null);
}
