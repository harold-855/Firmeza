using Firmeza.Application.Interfaces.Repositories;

namespace Firmeza.Application.UseCases.Ventas;

public class ActualizarEstadoDespachoUseCase(IUnitOfWork unitOfWork)
{
    public async Task<bool> ExecuteAsync(Guid id, string nuevoEstado, CancellationToken cancellationToken = default)
    {
        var venta = await unitOfWork.Ventas.GetByIdAsync(id, cancellationToken);
        if (venta == null) return false;

        venta.EstadoDespacho = nuevoEstado.Trim();
        await unitOfWork.Ventas.UpdateAsync(venta, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
