using Firmeza.Application.DTOS.Clientes;
using Firmeza.Application.Interfaces;
using Firmeza.Application.UseCases.Clientes;

namespace Firmeza.Application.Services;

public class ClienteService(
    ObtenerClientesUseCase obtenerClientesUseCase,
    CrearClienteUseCase crearClienteUseCase,
    ActualizarClienteUseCase actualizarClienteUseCase,
    EliminarClienteUseCase eliminarClienteUseCase) : IClienteService
{
    public Task<IEnumerable<ClienteDto>> GetAllAsync(ClienteFilterDto? filter = null)
        => obtenerClientesUseCase.ExecuteGetAllAsync(filter);

    public Task<ClienteDto?> GetByIdAsync(Guid id)
        => obtenerClientesUseCase.ExecuteGetByIdAsync(id);

    public Task<ClienteDto?> GetByDocumentoAsync(string documento)
        => obtenerClientesUseCase.ExecuteGetByDocumentoAsync(documento);

    public Task<ClienteDto> CreateAsync(CreateClienteDto dto)
        => crearClienteUseCase.ExecuteAsync(dto);

    public Task<bool> UpdateAsync(Guid id, UpdateClienteDto dto)
        => actualizarClienteUseCase.ExecuteAsync(id, dto);

    public Task<bool> DeleteAsync(Guid id)
        => eliminarClienteUseCase.ExecuteAsync(id);

    public Task<bool> ExistsAsync(Guid id)
        => obtenerClientesUseCase.ExecuteExistsAsync(id);

    public Task<bool> DocumentoExistsAsync(string documento, Guid? excludeId = null)
        => obtenerClientesUseCase.ExecuteDocumentoExistsAsync(documento, excludeId);
}
