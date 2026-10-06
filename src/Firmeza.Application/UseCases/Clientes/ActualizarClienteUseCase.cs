using Firmeza.Application.DTOS.Clientes;
using Firmeza.Application.Interfaces.Repositories;

namespace Firmeza.Application.UseCases.Clientes;

public class ActualizarClienteUseCase(IUnitOfWork unitOfWork)
{
    public async Task<bool> ExecuteAsync(Guid id, UpdateClienteDto dto, CancellationToken cancellationToken = default)
    {
        var cliente = await unitOfWork.Clientes.GetByIdAsync(id, cancellationToken);
        if (cliente == null)
            return false;

        cliente.DocumentoIdentidad = dto.DocumentoIdentidad.Trim();
        cliente.RazonSocial = dto.RazonSocial.Trim();
        cliente.Telefono = dto.Telefono.Trim();
        cliente.DireccionEnvio = dto.DireccionEnvio.Trim();
        cliente.Email = dto.Email.Trim().ToLower();

        await unitOfWork.Clientes.UpdateAsync(cliente, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
