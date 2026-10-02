using Firmeza.Application.DTOS.Clientes;
using Firmeza.Application.Interfaces;
using Firmeza.Application.Interfaces.Repositories;
using Firmeza.Domain.Entities;

namespace Firmeza.Infrastructure.Services;

public class ClienteService(IUnitOfWork unitOfWork) : IClienteService
{
    public async Task<IEnumerable<ClienteDto>> GetAllAsync(ClienteFilterDto? filter = null)
    {
        var clientes = await unitOfWork.Clientes.GetAllWithVentasAsync(filter);

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

    public async Task<ClienteDto?> GetByIdAsync(Guid id)
    {
        var c = await unitOfWork.Clientes.GetByIdWithVentasAsync(id);
        if (c == null) return null;

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

    public async Task<ClienteDto?> GetByDocumentoAsync(string documento)
    {
        var c = await unitOfWork.Clientes.GetByDocumentoAsync(documento);
        if (c == null) return null;

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

    public async Task<ClienteDto> CreateAsync(CreateClienteDto dto)
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

        await unitOfWork.Clientes.AddAsync(cliente);
        await unitOfWork.SaveChangesAsync();

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

    public async Task<bool> UpdateAsync(Guid id, UpdateClienteDto dto)
    {
        var cliente = await unitOfWork.Clientes.GetByIdAsync(id);
        if (cliente == null)
            return false;

        cliente.DocumentoIdentidad = dto.DocumentoIdentidad.Trim();
        cliente.RazonSocial = dto.RazonSocial.Trim();
        cliente.Telefono = dto.Telefono.Trim();
        cliente.DireccionEnvio = dto.DireccionEnvio.Trim();
        cliente.Email = dto.Email.Trim().ToLower();

        await unitOfWork.Clientes.UpdateAsync(cliente);
        await unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var cliente = await unitOfWork.Clientes.GetByIdWithVentasAsync(id);
        if (cliente == null)
            return false;

        if (cliente.Ventas.Count > 0)
        {
            throw new InvalidOperationException("No es posible eliminar el cliente porque cuenta con órdenes de venta y facturas históricas registradas.");
        }

        await unitOfWork.Clientes.DeleteAsync(cliente);
        await unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await unitOfWork.Clientes.ExistsAsync(id);
    }

    public async Task<bool> DocumentoExistsAsync(string documento, Guid? excludeId = null)
    {
        return await unitOfWork.Clientes.DocumentoExistsAsync(documento, excludeId);
    }
}
