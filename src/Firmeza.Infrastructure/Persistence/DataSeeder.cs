using Firmeza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Persistence;

public static class 
    DataSeeder
{
    public static async Task SeedSampleDataAsync(ApplicationDbContext context)
    {
        if (!await context.Clientes.AnyAsync())
        {
            var cliente1 = new Cliente
            {
                Id = Guid.NewGuid(),
                RazonSocial = "Constructora Los Andes S.A.S.",
                DocumentoIdentidad = "900123456-1",
                Telefono = "3001234567",
                Email = "compras@losandes.com",
                DireccionEnvio = "Calle 100 # 15-20, Bogotá"
            };

            var cliente2 = new Cliente
            {
                Id = Guid.NewGuid(),
                RazonSocial = "Ferretería El Progreso",
                DocumentoIdentidad = "800987654-3",
                Telefono = "3109876543",
                Email = "contacto@elprogreso.com",
                DireccionEnvio = "Carrera 45 # 10-30, Medellín"
            };

            var cliente3 = new Cliente
            {
                Id = Guid.NewGuid(),
                RazonSocial = "Inversiones Horizonte Ltda.",
                DocumentoIdentidad = "901234567-8",
                Telefono = "3204567890",
                Email = "gerencia@horizonte.com",
                DireccionEnvio = "Avenida 6N # 25-10, Cali"
            };

            await context.Clientes.AddRangeAsync(cliente1, cliente2, cliente3);

            var prod1 = new Producto
            {
                Id = Guid.NewGuid(),
                Nombre = "Cemento Gris Tipo 1 x 50kg",
                Descripcion = "Cemento de uso general para mampostería y estructuras",
                UnidadMedida = "Bolsa",
                PrecioUnitario = 32000m,
                StockActual = 150,
                Activo = true
            };

            var prod2 = new Producto
            {
                Id = Guid.NewGuid(),
                Nombre = "Varilla Corrugada 1/2 pulgada",
                Descripcion = "Acero de refuerzo figurado norma NTC 2289",
                UnidadMedida = "Unidad",
                PrecioUnitario = 45000m,
                StockActual = 80,
                Activo = true
            };

            var prod3 = new Producto
            {
                Id = Guid.NewGuid(),
                Nombre = "Ladrillo Estructurado Arcilla 10x20x40",
                Descripcion = "Ladrillo cocido estructural para muros",
                UnidadMedida = "Millar",
                PrecioUnitario = 1200000m,
                StockActual = 12,
                Activo = true
            };

            var prod4 = new Producto
            {
                Id = Guid.NewGuid(),
                Nombre = "Arena Lavada de Río (M3)",
                Descripcion = "Agregado fino para mezcla de morteros y concretos",
                UnidadMedida = "M3",
                PrecioUnitario = 85000m,
                StockActual = 35,
                Activo = true
            };

            await context.Productos.AddRangeAsync(prod1, prod2, prod3, prod4);

            var venta1 = new Venta
            {
                Id = Guid.NewGuid(),
                ClienteId = cliente1.Id,
                FechaVenta = DateTime.UtcNow.AddDays(-2),
                Total = 1600000m,
                EstadoDespacho = "Entregado",
                Detalles =
                [
                    new VentaDetalle
                    {
                        Id = Guid.NewGuid(),
                        ProductoId = prod1.Id,
                        Cantidad = 50,
                        PrecioAplicado = 32000m
                    }
                ]
            };

            var venta2 = new Venta
            {
                Id = Guid.NewGuid(),
                ClienteId = cliente2.Id,
                FechaVenta = DateTime.UtcNow.AddDays(-1),
                Total = 2250000m,
                EstadoDespacho = "En Ruta",
                Detalles =
                [
                    new VentaDetalle
                    {
                        Id = Guid.NewGuid(),
                        ProductoId = prod2.Id,
                        Cantidad = 50,
                        PrecioAplicado = 45000m
                    }
                ]
            };

            var venta3 = new Venta
            {
                Id = Guid.NewGuid(),
                ClienteId = cliente3.Id,
                FechaVenta = DateTime.UtcNow,
                Total = 1285000m,
                EstadoDespacho = "Pendiente",
                Detalles =
                [
                    new VentaDetalle
                    {
                        Id = Guid.NewGuid(),
                        ProductoId = prod3.Id,
                        Cantidad = 1,
                        PrecioAplicado = 1200000m
                    },
                    new VentaDetalle
                    {
                        Id = Guid.NewGuid(),
                        ProductoId = prod4.Id,
                        Cantidad = 1,
                        PrecioAplicado = 85000m
                    }
                ]
            };

            await context.Ventas.AddRangeAsync(venta1, venta2, venta3);
            await context.SaveChangesAsync();
        }
    }
}
