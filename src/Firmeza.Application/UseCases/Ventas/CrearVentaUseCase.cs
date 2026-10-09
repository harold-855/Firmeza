using AutoMapper;
using Firmeza.Application.DTOS.Ventas;
using Firmeza.Application.Interfaces;
using Firmeza.Application.Interfaces.Repositories;
using Firmeza.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Firmeza.Application.UseCases.Ventas;

public class CrearVentaUseCase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExportService _exportService;
    private readonly ILogger<CrearVentaUseCase> _logger;
    private readonly IEmailService? _emailService;
    private readonly IMapper? _mapper;

    public CrearVentaUseCase(
        IUnitOfWork unitOfWork,
        IExportService exportService,
        ILogger<CrearVentaUseCase> logger,
        IEmailService? emailService = null,
        IMapper? mapper = null)
    {
        _unitOfWork = unitOfWork;
        _exportService = exportService;
        _logger = logger;
        _emailService = emailService;
        _mapper = mapper;
    }

    public CrearVentaUseCase(
        IUnitOfWork unitOfWork,
        IExportService exportService,
        ILogger<CrearVentaUseCase> logger,
        IMapper? mapper)
        : this(unitOfWork, exportService, logger, null, mapper)
    {
    }

    public async Task<VentaDto> ExecuteAsync(CreateVentaDto dto, string? wwwrootPath = null, CancellationToken cancellationToken = default)
    {
        Cliente? cliente = null;

        if (dto.ClienteId != Guid.Empty)
        {
            cliente = await _unitOfWork.Clientes.GetByIdAsync(dto.ClienteId, cancellationToken);
        }

        if (cliente == null && !string.IsNullOrWhiteSpace(dto.ClienteEmail))
        {
            var allClientes = await _unitOfWork.Clientes.GetAllAsync(cancellationToken: cancellationToken);
            cliente = allClientes.FirstOrDefault(c => string.Equals(c.Email, dto.ClienteEmail, StringComparison.OrdinalIgnoreCase));
        }

        if (cliente == null)
        {
            var clienteId = dto.ClienteId != Guid.Empty ? dto.ClienteId : Guid.NewGuid();
            var email = !string.IsNullOrWhiteSpace(dto.ClienteEmail) ? dto.ClienteEmail.Trim() : "cliente@firmeza.com";
            var rawName = email.Contains('@') ? email.Split('@')[0] : "Cliente Web";
            var formattedName = char.ToUpper(rawName[0]) + (rawName.Length > 1 ? rawName[1..] : "");

            cliente = new Cliente
            {
                Id = clienteId,
                DocumentoIdentidad = "NIT-" + clienteId.ToString()[..8].ToUpperInvariant(),
                RazonSocial = formattedName,
                Email = email,
                Telefono = "3001234567",
                DireccionEnvio = "Dirección de Despacho Principal"
            };

            await _unitOfWork.Clientes.AddAsync(cliente, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

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
            await _unitOfWork.Productos.UpdateAsync(producto, cancellationToken);

            venta.Detalles.Add(detalle);
            totalVenta += (det.Cantidad * precioUnitarioAplicado);
        }

        venta.Total = totalVenta;

        await _unitOfWork.Ventas.AddAsync(venta, cancellationToken);
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

        // Envío defensivo de correo de confirmación de compra con recibo adjunto
        var targetEmail = !string.IsNullOrWhiteSpace(dto.ClienteEmail) ? dto.ClienteEmail.Trim() : cliente.Email;
        if (_emailService != null && !string.IsNullOrWhiteSpace(targetEmail))
        {
            try
            {
                byte[]? pdfBytes = null;
                try
                {
                    pdfBytes = await _exportService.GenerarComprobanteReciboPdfAsync(venta.Id, cancellationToken);
                }
                catch (Exception exPdf)
                {
                    _logger.LogWarning(exPdf, "No se pudo generar el PDF adjunto para el correo de la venta {VentaId}", venta.Id);
                }

                await _emailService.SendPurchaseConfirmationEmailAsync(
                    targetEmail,
                    cliente.RazonSocial,
                    venta.Id,
                    venta.Total,
                    venta.EstadoDespacho,
                    pdfBytes,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar la notificación de confirmación de compra al correo {Email}", targetEmail);
            }
        }

        var resultDto = _mapper != null ? _mapper.Map<VentaDto>(venta) : MapToDto(venta);
        resultDto.RutaArchivoRecibo = rutaRecibo;
        if (!string.IsNullOrWhiteSpace(targetEmail))
        {
            resultDto.ClienteEmail = targetEmail;
        }
        return resultDto;
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
