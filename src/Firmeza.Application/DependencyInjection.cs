using Firmeza.Application.Interfaces;
using Firmeza.Application.Services;
using Firmeza.Application.UseCases.Clientes;
using Firmeza.Application.UseCases.Dashboard;
using Firmeza.Application.UseCases.Productos;
using Firmeza.Application.UseCases.Ventas;
using Microsoft.Extensions.DependencyInjection;

namespace Firmeza.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // 1. Registrar AutoMapper con los perfiles de mapeo del ensamblado Application
        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(DependencyInjection).Assembly));

        // 2. Registrar Casos de Uso (Use Cases) de Productos
        services.AddScoped<CrearProductoUseCase>();
        services.AddScoped<ActualizarProductoUseCase>();
        services.AddScoped<EliminarProductoUseCase>();
        services.AddScoped<ObtenerProductosUseCase>();

        // 3. Registrar Casos de Uso (Use Cases) de Clientes
        services.AddScoped<CrearClienteUseCase>();
        services.AddScoped<ActualizarClienteUseCase>();
        services.AddScoped<EliminarClienteUseCase>();
        services.AddScoped<ObtenerClientesUseCase>();

        // 4. Registrar Casos de Uso (Use Cases) de Ventas
        services.AddScoped<CrearVentaUseCase>();
        services.AddScoped<ObtenerVentasUseCase>();
        services.AddScoped<ActualizarEstadoDespachoUseCase>();

        // 5. Registrar Casos de Uso (Use Cases) de Dashboard
        services.AddScoped<ObtenerDashboardMetricsUseCase>();

        // 6. Registrar Fachadas / Servicios de Aplicación
        services.AddScoped<IProductoService, ProductoService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IVentaService, VentaService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
