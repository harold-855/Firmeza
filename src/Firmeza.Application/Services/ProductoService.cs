using Firmeza.Application.DTOS.Productos;
using Firmeza.Application.Interfaces;
using Firmeza.Application.Interfaces.Repositories;
using Firmeza.Application.UseCases.Productos;

namespace Firmeza.Application.Services;

public class ProductoService(
    ObtenerProductosUseCase obtenerProductosUseCase,
    CrearProductoUseCase crearProductoUseCase,
    ActualizarProductoUseCase actualizarProductoUseCase,
    EliminarProductoUseCase eliminarProductoUseCase) : IProductoService
{
    public ProductoService(IUnitOfWork unitOfWork)
        : this(
            new ObtenerProductosUseCase(unitOfWork),
            new CrearProductoUseCase(unitOfWork),
            new ActualizarProductoUseCase(unitOfWork),
            new EliminarProductoUseCase(unitOfWork))
    {
    }

    public Task<IEnumerable<ProductoDto>> GetAllAsync(ProductoFilterDto? filter = null)
        => obtenerProductosUseCase.ExecuteGetAllAsync(filter);

    public Task<ProductoDto?> GetByIdAsync(Guid id)
        => obtenerProductosUseCase.ExecuteGetByIdAsync(id);

    public Task<ProductoDto> CreateAsync(CreateProductoDto dto)
        => crearProductoUseCase.ExecuteAsync(dto);

    public Task<bool> UpdateAsync(Guid id, UpdateProductoDto dto)
        => actualizarProductoUseCase.ExecuteAsync(id, dto);

    public Task<bool> DeleteAsync(Guid id)
        => eliminarProductoUseCase.ExecuteAsync(id);

    public Task<bool> ExistsAsync(Guid id)
        => obtenerProductosUseCase.ExecuteExistsAsync(id);

    public Task<IEnumerable<string>> GetUnidadesMedidaAsync()
        => obtenerProductosUseCase.ExecuteGetUnidadesMedidaAsync();
}
