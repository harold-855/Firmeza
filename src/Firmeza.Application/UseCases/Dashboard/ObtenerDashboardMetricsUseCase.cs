using Firmeza.Application.DTOS.Dashboard;
using Firmeza.Application.Interfaces.Repositories;

namespace Firmeza.Application.UseCases.Dashboard;

public class ObtenerDashboardMetricsUseCase(IUnitOfWork unitOfWork)
{
    public async Task<DashboardMetricsDto> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var productos = await unitOfWork.Productos.GetAllAsync(cancellationToken);
        var clientes = await unitOfWork.Clientes.GetAllAsync(cancellationToken);
        var ventas = (await unitOfWork.Ventas.GetAllWithDetailsAsync(cancellationToken)).ToList();

        var totalProductos = productos.Count;
        var totalClientes = clientes.Count;
        var totalVentas = ventas.Count;
        var montoTotalVentas = ventas.Sum(v => v.Total);

        var pendientes = ventas.Count(v => v.EstadoDespacho.Equals("Pendiente", StringComparison.OrdinalIgnoreCase));
        var enRuta = ventas.Count(v => v.EstadoDespacho.Equals("En Ruta", StringComparison.OrdinalIgnoreCase));
        var entregados = ventas.Count(v => v.EstadoDespacho.Equals("Entregado", StringComparison.OrdinalIgnoreCase));

        var ventasRecientes = ventas
            .OrderByDescending(v => v.FechaVenta)
            .Take(5)
            .Select(v => new VentaResumenDto
            {
                Id = v.Id,
                ClienteNombre = v.Cliente != null ? v.Cliente.RazonSocial : "Cliente General",
                Fecha = v.FechaVenta,
                Total = v.Total,
                EstadoDespacho = v.EstadoDespacho
            })
            .ToList();

        var productosBajoStock = productos
            .Where(p => p.StockActual < 50)
            .OrderBy(p => p.StockActual)
            .Take(5)
            .Select(p => new ProductoResumenDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                StockActual = p.StockActual,
                PrecioUnitario = p.PrecioUnitario,
                UnidadMedida = p.UnidadMedida
            })
            .ToList();

        return new DashboardMetricsDto
        {
            TotalProductos = totalProductos,
            TotalClientes = totalClientes,
            TotalVentas = totalVentas,
            MontoTotalVentas = montoTotalVentas,
            DespachosPendientes = pendientes,
            DespachosEnRuta = enRuta,
            DespachosEntregados = entregados,
            VentasRecientes = ventasRecientes,
            ProductosBajoStock = productosBajoStock
        };
    }
}
