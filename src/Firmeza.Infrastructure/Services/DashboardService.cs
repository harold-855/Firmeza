using Firmeza.Application.DTOS.Dashboard;
using Firmeza.Application.Interfaces;
using Firmeza.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Services;

public class DashboardService(ApplicationDbContext context) : IDashboardService
{
    public async Task<DashboardMetricsDto> GetDashboardMetricsAsync()
    {
        var totalProductos = await context.Productos.CountAsync();
        var totalClientes = await context.Clientes.CountAsync();
        var totalVentas = await context.Ventas.CountAsync();
        var montoTotalVentas = await context.Ventas.SumAsync(v => (decimal?)v.Total) ?? 0m;

        var pendientes = await context.Ventas.CountAsync(v => v.EstadoDespacho == "Pendiente");
        var enRuta = await context.Ventas.CountAsync(v => v.EstadoDespacho == "En Ruta");
        var entregados = await context.Ventas.CountAsync(v => v.EstadoDespacho == "Entregado");

        var ventasRecientes = await context.Ventas
            .Include(v => v.Cliente)
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
            .ToListAsync();

        var productosBajoStock = await context.Productos
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
            .ToListAsync();

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
