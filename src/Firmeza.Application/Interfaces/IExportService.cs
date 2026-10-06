using Firmeza.Application.DTOS.Clientes;
using Firmeza.Application.DTOS.Productos;
using Firmeza.Application.DTOS.Ventas;

namespace Firmeza.Application.Interfaces;

public interface IExportService
{
    // Exportaciones de Productos
    Task<byte[]> ExportarProductosExcelAsync(ProductoFilterDto? filter = null, CancellationToken cancellationToken = default);
    Task<byte[]> ExportarProductosPdfAsync(ProductoFilterDto? filter = null, CancellationToken cancellationToken = default);

    // Exportaciones de Clientes
    Task<byte[]> ExportarClientesExcelAsync(ClienteFilterDto? filter = null, CancellationToken cancellationToken = default);
    Task<byte[]> ExportarClientesPdfAsync(ClienteFilterDto? filter = null, CancellationToken cancellationToken = default);

    // Exportaciones de Ventas
    Task<byte[]> ExportarVentasExcelAsync(VentaFilterDto? filter = null, CancellationToken cancellationToken = default);
    Task<byte[]> ExportarVentasPdfAsync(VentaFilterDto? filter = null, CancellationToken cancellationToken = default);

    // Comprobante / Recibo de Venta Individual
    Task<byte[]> GenerarComprobanteReciboPdfAsync(Guid ventaId, CancellationToken cancellationToken = default);
    Task<string> GuardarComprobanteReciboAsync(Guid ventaId, string wwwrootPath, CancellationToken cancellationToken = default);
}
