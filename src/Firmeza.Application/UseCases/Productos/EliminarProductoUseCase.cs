using Firmeza.Application.Interfaces.Repositories;

namespace Firmeza.Application.UseCases.Productos;

public class EliminarProductoUseCase(IUnitOfWork unitOfWork)
{
    public async Task<bool> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var producto = await unitOfWork.Productos.GetByIdWithDetallesAsync(id, cancellationToken);
        if (producto == null)
            return false;

        // Regla de negocio: Si el producto tiene ventas asociadas, se desactiva lógicamente para proteger el historial contable
        if (producto.DetallesVenta.Count > 0)
        {
            producto.Activo = false;
            await unitOfWork.Productos.UpdateAsync(producto, cancellationToken);
        }
        else
        {
            await unitOfWork.Productos.DeleteAsync(producto, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
