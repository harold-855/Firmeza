using AutoMapper;
using Firmeza.Application.DTOS.Clientes;
using Firmeza.Application.Interfaces.Repositories;

namespace Firmeza.Application.UseCases.Clientes;

public class ObtenerClientesUseCase(IUnitOfWork unitOfWork, IMapper? mapper = null)
{
    public async Task<IEnumerable<ClienteDto>> ExecuteGetAllAsync(ClienteFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        var clientes = await unitOfWork.Clientes.GetAllWithVentasAsync(filter, cancellationToken);

        if (mapper != null)
        {
            return mapper.Map<IEnumerable<ClienteDto>>(clientes);
        }

        return clientes.Select(c => new ClienteDto
        {
            Id = c.Id,
            DocumentoIdentidad = c.DocumentoIdentidad,
            RazonSocial = c.RazonSocial,
            Telefono = c.Telefono,
            DireccionEnvio = c.DireccionEnvio,
            Email = c.Email,
            TotalCompras = c.Ventas.Count,
            MontoTotalComprado = c.Ventas.Sum(v => (decimal?)v.Total) ?? 0m
        }).ToList();
    }

    public async Task<ClienteDto?> ExecuteGetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var c = await unitOfWork.Clientes.GetByIdWithVentasAsync(id, cancellationToken);
        if (c == null) return null;

        if (mapper != null)
        {
            return mapper.Map<ClienteDto>(c);
        }

        return new ClienteDto
        {
            Id = c.Id,
            DocumentoIdentidad = c.DocumentoIdentidad,
            RazonSocial = c.RazonSocial,
            Telefono = c.Telefono,
            DireccionEnvio = c.DireccionEnvio,
            Email = c.Email,
            TotalCompras = c.Ventas.Count,
            MontoTotalComprado = c.Ventas.Sum(v => (decimal?)v.Total) ?? 0m
        };
    }

    public async Task<ClienteDto?> ExecuteGetByDocumentoAsync(string documento, CancellationToken cancellationToken = default)
    {
        var c = await unitOfWork.Clientes.GetByDocumentoAsync(documento, cancellationToken);
        if (c == null) return null;

        if (mapper != null)
        {
            return mapper.Map<ClienteDto>(c);
        }

        return new ClienteDto
        {
            Id = c.Id,
            DocumentoIdentidad = c.DocumentoIdentidad,
            RazonSocial = c.RazonSocial,
            Telefono = c.Telefono,
            DireccionEnvio = c.DireccionEnvio,
            Email = c.Email,
            TotalCompras = c.Ventas.Count,
            MontoTotalComprado = c.Ventas.Sum(v => (decimal?)v.Total) ?? 0m
        };
    }

    public async Task<bool> ExecuteExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await unitOfWork.Clientes.ExistsAsync(id, cancellationToken);
    }

    public async Task<bool> ExecuteDocumentoExistsAsync(string documento, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        return await unitOfWork.Clientes.DocumentoExistsAsync(documento, excludeId, cancellationToken);
    }
}
