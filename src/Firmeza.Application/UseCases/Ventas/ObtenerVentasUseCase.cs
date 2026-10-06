using Firmeza.Application.DTOS.Ventas;
using Firmeza.Application.Interfaces.Repositories;
using Firmeza.Domain.Entities;

namespace Firmeza.Application.UseCases.Ventas;

public class ObtenerVentasUseCase(IUnitOfWork unitOfWork)
{
    public async Task<IEnumerable<VentaDto>> ExecuteGetAllAsync(VentaFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        var ventas = (await unitOfWork.Ventas.GetAllWithDetailsAsync(cancellationToken)).ToList();

        if (filter != null)
        {
            if (filter.ClienteId.HasValue)
            {
                ventas = ventas.Where(v => v.ClienteId == filter.ClienteId.Value).ToList();
            }

            if (!string.IsNullOrWhiteSpace(filter.EstadoDespacho))
            {
                ventas = ventas.Where(v => v.EstadoDespacho.Equals(filter.EstadoDespacho, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (filter.FechaInicio.HasValue)
            {
                ventas = ventas.Where(v => v.FechaVenta >= filter.FechaInicio.Value).ToList();
            }

            if (filter.FechaFin.HasValue)
            {
                ventas = ventas.Where(v => v.FechaVenta <= filter.FechaFin.Value).ToList();
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLowerInvariant();
                ventas = ventas.Where(v =>
                    (v.Cliente != null && (v.Cliente.RazonSocial.ToLowerInvariant().Contains(term) || v.Cliente.DocumentoIdentidad.ToLowerInvariant().Contains(term))) ||
                    v.Id.ToString().ToLowerInvariant().Contains(term) ||
                    v.EstadoDespacho.ToLowerInvariant().Contains(term) ||
                    v.Detalles.Any(d => d.Producto != null && d.Producto.Nombre.ToLowerInvariant().Contains(term))
                ).ToList();
            }

            ventas = filter.OrderBy switch
            {
                "fecha_asc" => ventas.OrderBy(v => v.FechaVenta).ToList(),
                "total_desc" => ventas.OrderByDescending(v => v.Total).ToList(),
                "total_asc" => ventas.OrderBy(v => v.Total).ToList(),
                "cliente_asc" => ventas.OrderBy(v => v.Cliente?.RazonSocial).ToList(),
                _ => ventas.OrderByDescending(v => v.FechaVenta).ToList()
            };
        }

        return ventas.Select(MapToDto).ToList();
    }

    public async Task<VentaDto?> ExecuteGetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var venta = await unitOfWork.Ventas.GetByIdWithDetailsAsync(id, cancellationToken);
        return venta == null ? null : MapToDto(venta);
    }

    private static VentaDto MapToDto(Venta v)
    {
        return new VentaDto
        {
            Id = v.Id,
            FechaVenta = v.FechaVenta,
            Total = v.Total,
            EstadoDespacho = v.EstadoDespacho,
            ClienteId = v.ClienteId,
            ClienteRazonSocial = v.Cliente?.RazonSocial ?? "Cliente General",
            ClienteDocumento = v.Cliente?.DocumentoIdentidad ?? "",
            ClienteTelefono = v.Cliente?.Telefono ?? "",
            ClienteDireccion = v.Cliente?.DireccionEnvio ?? "",
            ClienteEmail = v.Cliente?.Email ?? "",
            Detalles = v.Detalles.Select(d => new VentaDetalleDto
            {
                Id = d.Id,
                ProductoId = d.ProductoId,
                ProductoNombre = d.Producto?.Nombre ?? "Material General",
                UnidadMedida = d.Producto?.UnidadMedida ?? "UND",
                Cantidad = d.Cantidad,
                PrecioAplicado = d.PrecioAplicado
            }).ToList()
        };
    }
}
