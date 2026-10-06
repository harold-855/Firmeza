using Firmeza.Application.DTOS.Ventas;
using Firmeza.Application.Interfaces;
using Firmeza.Application.Interfaces.Repositories;
using Firmeza.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Firmeza.Infrastructure.Services;

public class VentaService(
    IUnitOfWork unitOfWork,
    IExportService exportService,
    ILogger<VentaService> logger) : IVentaService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IExportService _exportService = exportService;
    private readonly ILogger<VentaService> _logger = logger;

    public async Task<IEnumerable<VentaDto>> GetAllAsync(VentaFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        var ventas = (await _unitOfWork.Ventas.GetAllWithDetailsAsync(cancellationToken)).ToList();

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

    public async Task<VentaDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var venta = await _unitOfWork.Ventas.GetByIdWithDetailsAsync(id, cancellationToken);
        return venta == null ? null : MapToDto(venta);
    }

    public async Task<VentaDto> CreateAsync(CreateVentaDto dto, string? wwwrootPath = null, CancellationToken cancellationToken = default)
    {
        if (dto.ClienteId == Guid.Empty)
        {
            throw new ArgumentException("Debe especificar un cliente válido para la venta.", nameof(dto));
        }

        var cliente = await _unitOfWork.Clientes.GetByIdAsync(dto.ClienteId, cancellationToken)
            ?? throw new KeyNotFoundException($"El cliente con Id '{dto.ClienteId}' no fue encontrado.");

        if (dto.Detalles == null || dto.Detalles.Count == 0)
        {
            throw new ArgumentException("La orden de venta debe contener al menos un producto.", nameof(dto));
        }

        var venta = new Venta
        {
            Id = Guid.NewGuid(),
            ClienteId = cliente.Id,
            Cliente = cliente,
            FechaVenta = DateTime.UtcNow,
            EstadoDespacho = string.IsNullOrWhiteSpace(dto.EstadoDespacho) ? "Pendiente" : dto.EstadoDespacho.Trim(),
            Total = 0
        };

        decimal totalVenta = 0;

        foreach (var det in dto.Detalles)
        {
            if (det.Cantidad <= 0)
            {
                throw new ArgumentException("La cantidad de cada ítem debe ser mayor a 0.");
            }

            var producto = await _unitOfWork.Productos.GetByIdAsync(det.ProductoId, cancellationToken)
                ?? throw new KeyNotFoundException($"El producto con Id '{det.ProductoId}' no fue encontrado.");

            decimal precioUnitarioAplicado = det.PrecioAplicado.HasValue && det.PrecioAplicado.Value >= 0
                ? det.PrecioAplicado.Value
                : producto.PrecioUnitario;

            var detalle = new VentaDetalle
            {
                Id = Guid.NewGuid(),
                VentaId = venta.Id,
                Venta = venta,
                ProductoId = producto.Id,
                Producto = producto,
                Cantidad = det.Cantidad,
                PrecioAplicado = precioUnitarioAplicado
            };

            // Descontar inventario disponible
            producto.StockActual = Math.Max(0, producto.StockActual - det.Cantidad);
            await _unitOfWork.Productos.UpdateAsync(producto);

            venta.Detalles.Add(detalle);
            totalVenta += (det.Cantidad * precioUnitarioAplicado);
        }

        venta.Total = totalVenta;

        await _unitOfWork.Ventas.AddAsync(venta);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        string? rutaRecibo = null;

        // Generar y almacenar el comprobante en wwwroot/recibos si se proporciona la ruta web
        if (!string.IsNullOrWhiteSpace(wwwrootPath))
        {
            try
            {
                rutaRecibo = await _exportService.GuardarComprobanteReciboAsync(venta.Id, wwwrootPath, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo guardar automáticamente el comprobante PDF para la venta {VentaId}", venta.Id);
            }
        }

        var resultDto = MapToDto(venta);
        resultDto.RutaArchivoRecibo = rutaRecibo;
        return resultDto;
    }

    public async Task<bool> UpdateEstadoDespachoAsync(Guid id, string nuevoEstado, CancellationToken cancellationToken = default)
    {
        var venta = await _unitOfWork.Ventas.GetByIdAsync(id, cancellationToken);
        if (venta == null) return false;

        venta.EstadoDespacho = nuevoEstado.Trim();
        await _unitOfWork.Ventas.UpdateAsync(venta);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
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
