using Firmeza.Application.DTOS.Productos;

namespace Firmeza.Application.Interfaces;

public interface IProductoService
{
    Task<IEnumerable<ProductoDto>> GetAllAsync(ProductoFilterDto? filter = null);
    Task<ProductoDto?> GetByIdAsync(Guid id);
    Task<ProductoDto> CreateAsync(CreateProductoDto dto);
    Task<bool> UpdateAsync(Guid id, UpdateProductoDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<IEnumerable<string>> GetUnidadesMedidaAsync();
}
