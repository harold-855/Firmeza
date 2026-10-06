using Firmeza.Application.Interfaces.Repositories;

namespace Firmeza.Application.UseCases.Clientes;

public class EliminarClienteUseCase(IUnitOfWork unitOfWork)
{
    public async Task<bool> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cliente = await unitOfWork.Clientes.GetByIdWithVentasAsync(id, cancellationToken);
        if (cliente == null)
            return false;

        if (cliente.Ventas.Count > 0)
        {
            throw new InvalidOperationException("No es posible eliminar el cliente porque cuenta con órdenes de venta y facturas históricas registradas.");
        }

        await unitOfWork.Clientes.DeleteAsync(cliente, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
