using Firmeza.Application.DTOS.Clientes;
using Firmeza.Application.Interfaces.Repositories;
using Firmeza.Domain.Entities;

namespace Firmeza.Application.UseCases.Clientes;

public class CrearClienteUseCase(IUnitOfWork unitOfWork)
{
    public async Task<ClienteDto> ExecuteAsync(CreateClienteDto dto, CancellationToken cancellationToken = default)
    {
        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            DocumentoIdentidad = dto.DocumentoIdentidad.Trim(),
            RazonSocial = dto.RazonSocial.Trim(),
            Telefono = dto.Telefono.Trim(),
            DireccionEnvio = dto.DireccionEnvio.Trim(),
            Email = dto.Email.Trim().ToLower()
        };

        await unitOfWork.Clientes.AddAsync(cliente, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ClienteDto
        {
            Id = cliente.Id,
            DocumentoIdentidad = cliente.DocumentoIdentidad,
            RazonSocial = cliente.RazonSocial,
            Telefono = cliente.Telefono,
            DireccionEnvio = cliente.DireccionEnvio,
            Email = cliente.Email,
            TotalCompras = 0,
            MontoTotalComprado = 0m
        };
    }
}
