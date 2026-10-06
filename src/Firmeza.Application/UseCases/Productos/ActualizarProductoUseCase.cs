using Firmeza.Application.DTOS.Productos;
using Firmeza.Application.Interfaces.Repositories;

namespace Firmeza.Application.UseCases.Productos;

public class ActualizarProductoUseCase(IUnitOfWork unitOfWork)
{
    public async Task<bool> ExecuteAsync(Guid id, UpdateProductoDto dto, CancellationToken cancellationToken = default)
    {
        var producto = await unitOfWork.Productos.GetByIdAsync(id, cancellationToken);
        if (producto == null)
            return false;

        producto.Nombre = dto.Nombre.Trim();
        producto.Descripcion = dto.Descripcion.Trim();
        producto.UnidadMedida = dto.UnidadMedida.Trim();
        producto.PrecioUnitario = dto.PrecioUnitario;
        producto.StockActual = dto.StockActual;
        producto.Activo = dto.Activo;

        await unitOfWork.Productos.UpdateAsync(producto, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
