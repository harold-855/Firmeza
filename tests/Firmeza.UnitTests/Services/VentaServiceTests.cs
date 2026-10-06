using Firmeza.Application.DTOS.Ventas;
using Firmeza.Application.Services;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Firmeza.Infrastructure.Repositories;
using Firmeza.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Firmeza.UnitTests.Services;

public class VentaServiceTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_VentaValida_DebeRegistrarVentaDescontarStockYGenerarTotalesEIVA()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var clienteRepo = new ClienteRepository(dbContext);
        var productoRepo = new ProductoRepository(dbContext);
        var ventaRepo = new VentaRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext, clienteRepo, productoRepo, ventaRepo);
        var exportService = new ExportService(unitOfWork);
        var service = new VentaService(unitOfWork, exportService, NullLogger<VentaService>.Instance);

        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            DocumentoIdentidad = "901555666-7",
            RazonSocial = "Constructora del Sol",
            Telefono = "3159988776",
            DireccionEnvio = "Carrera 15 # 85-30",
            Email = "compras@constructoradelsol.com"
        };
        var producto = new Producto
        {
            Id = Guid.NewGuid(),
            Nombre = "Cemento Blanco x 40kg",
            PrecioUnitario = 55000m,
            StockActual = 100,
            Activo = true
        };

        await dbContext.Clientes.AddAsync(cliente);
        await dbContext.Productos.AddAsync(producto);
        await dbContext.SaveChangesAsync();

        var createDto = new CreateVentaDto
        {
            ClienteId = cliente.Id,
            EstadoDespacho = "Pendiente",
            Detalles = new List<CreateVentaDetalleDto>
            {
                new()
                {
                    ProductoId = producto.Id,
                    Cantidad = 20,
                    PrecioAplicado = 55000m
                }
            }
        };

        var tempDir = Path.Combine(Path.GetTempPath(), "firmeza_test_venta_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act
            var ventaDto = await service.CreateAsync(createDto, tempDir);

            // Assert
            Assert.NotNull(ventaDto);
            Assert.Equal(cliente.Id, ventaDto.ClienteId);
            Assert.Equal(cliente.RazonSocial, ventaDto.ClienteRazonSocial);
            // 20 * 55000 = 1,100,000
            Assert.Equal(1100000m, ventaDto.Total);
            Assert.Equal(Math.Round(1100000m / 1.19m, 2), ventaDto.SubtotalBase);
            Assert.Equal(1100000m - ventaDto.SubtotalBase, ventaDto.Iva);
            Assert.Equal("Pendiente", ventaDto.EstadoDespacho);
            Assert.NotNull(ventaDto.RutaArchivoRecibo);

            // Verificar persistencia en base de datos
            var ventaDb = await dbContext.Ventas.Include(v => v.Detalles).FirstOrDefaultAsync(v => v.Id == ventaDto.Id);
            Assert.NotNull(ventaDb);
            Assert.Equal(1100000m, ventaDb.Total);
            Assert.Single(ventaDb.Detalles);

            // Verificar descuento de stock
            var productoDb = await dbContext.Productos.FindAsync(producto.Id);
            Assert.NotNull(productoDb);
            Assert.Equal(80, productoDb.StockActual); // 100 - 20 = 80

            // Verificar archivo físico de comprobante PDF en tempDir
            var expectedFile = Path.Combine(tempDir, "recibos", $"recibo_{ventaDto.Id}.pdf");
            Assert.True(File.Exists(expectedFile));
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public async Task GetAllAsync_ConFiltros_DebeRetornarVentasFiltradas()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var clienteRepo = new ClienteRepository(dbContext);
        var productoRepo = new ProductoRepository(dbContext);
        var ventaRepo = new VentaRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext, clienteRepo, productoRepo, ventaRepo);
        var exportService = new ExportService(unitOfWork);
        var service = new VentaService(unitOfWork, exportService, NullLogger<VentaService>.Instance);

        var cliente1 = new Cliente { Id = Guid.NewGuid(), RazonSocial = "Empresa Alfa", DocumentoIdentidad = "111" };
        var cliente2 = new Cliente { Id = Guid.NewGuid(), RazonSocial = "Empresa Beta", DocumentoIdentidad = "222" };
        var producto = new Producto { Id = Guid.NewGuid(), Nombre = "Arena", PrecioUnitario = 50000m };

        var venta1 = new Venta { Id = Guid.NewGuid(), ClienteId = cliente1.Id, Cliente = cliente1, Total = 500000m, EstadoDespacho = "Entregado", FechaVenta = DateTime.UtcNow.AddDays(-5) };
        var venta2 = new Venta { Id = Guid.NewGuid(), ClienteId = cliente2.Id, Cliente = cliente2, Total = 1200000m, EstadoDespacho = "Pendiente", FechaVenta = DateTime.UtcNow };

        await dbContext.Clientes.AddRangeAsync(cliente1, cliente2);
        await dbContext.Productos.AddAsync(producto);
        await dbContext.Ventas.AddRangeAsync(venta1, venta2);
        await dbContext.SaveChangesAsync();

        // Act
        var entregadas = await service.GetAllAsync(new VentaFilterDto { EstadoDespacho = "Entregado" });
        var porBusqueda = await service.GetAllAsync(new VentaFilterDto { SearchTerm = "Beta" });

        // Assert
        Assert.Single(entregadas);
        Assert.Equal("Entregado", entregadas.First().EstadoDespacho);

        Assert.Single(porBusqueda);
        Assert.Equal("Empresa Beta", porBusqueda.First().ClienteRazonSocial);
    }
}
