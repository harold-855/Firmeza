using Firmeza.Application.DTOS.Ventas;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Firmeza.Infrastructure.Repositories;
using Firmeza.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Xunit;

namespace Firmeza.UnitTests.Services;

public class ExportServiceTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task ExportarProductosExcel_DebeRetornarBytesValidosDeExcelConDatos()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var clienteRepo = new ClienteRepository(dbContext);
        var productoRepo = new ProductoRepository(dbContext);
        var ventaRepo = new VentaRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext, clienteRepo, productoRepo, ventaRepo);
        var service = new ExportService(unitOfWork);

        await dbContext.Productos.AddAsync(new Producto
        {
            Id = Guid.NewGuid(),
            Nombre = "Cemento Gris Tipo 1",
            Descripcion = "Bolsa 50kg",
            UnidadMedida = "Bolsa",
            PrecioUnitario = 32000m,
            StockActual = 100,
            Activo = true
        });
        await dbContext.SaveChangesAsync();

        // Act
        var bytes = await service.ExportarProductosExcelAsync();

        // Assert
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);

        using var stream = new MemoryStream(bytes);
        using var package = new ExcelPackage(stream);
        var ws = package.Workbook.Worksheets["Catálogo de Productos"];
        Assert.NotNull(ws);
        Assert.Equal("Cemento Gris Tipo 1", ws.Cells[4, 2].Value?.ToString());
    }

    [Fact]
    public async Task ExportarProductosPdf_DebeGenerarBytesDeDocumentoPdf()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var clienteRepo = new ClienteRepository(dbContext);
        var productoRepo = new ProductoRepository(dbContext);
        var ventaRepo = new VentaRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext, clienteRepo, productoRepo, ventaRepo);
        var service = new ExportService(unitOfWork);

        await dbContext.Productos.AddAsync(new Producto
        {
            Id = Guid.NewGuid(),
            Nombre = "Varilla Corrugada 1/2",
            UnidadMedida = "Unidad",
            PrecioUnitario = 45000m,
            StockActual = 80,
            Activo = true
        });
        await dbContext.SaveChangesAsync();

        // Act
        var bytes = await service.ExportarProductosPdfAsync();

        // Assert
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
        // Verificar que empiece con la firma mágica de archivo PDF (%PDF)
        Assert.Equal((byte)'%', bytes[0]);
        Assert.Equal((byte)'P', bytes[1]);
        Assert.Equal((byte)'D', bytes[2]);
        Assert.Equal((byte)'F', bytes[3]);
    }

    [Fact]
    public async Task ExportarClientesExcelYPdf_DebeGenerarArchivosCorrectamente()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var clienteRepo = new ClienteRepository(dbContext);
        var productoRepo = new ProductoRepository(dbContext);
        var ventaRepo = new VentaRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext, clienteRepo, productoRepo, ventaRepo);
        var service = new ExportService(unitOfWork);

        await dbContext.Clientes.AddAsync(new Cliente
        {
            Id = Guid.NewGuid(),
            DocumentoIdentidad = "900987654-1",
            RazonSocial = "Constructora Capital S.A.",
            Telefono = "3101234567",
            Email = "compras@capital.com"
        });
        await dbContext.SaveChangesAsync();

        // Act
        var excelBytes = await service.ExportarClientesExcelAsync();
        var pdfBytes = await service.ExportarClientesPdfAsync();

        // Assert
        Assert.NotNull(excelBytes);
        Assert.True(excelBytes.Length > 0);

        Assert.NotNull(pdfBytes);
        Assert.True(pdfBytes.Length > 0);
        Assert.Equal((byte)'%', pdfBytes[0]);
    }

    [Fact]
    public async Task GenerarYGuardarComprobanteReciboPdf_DebeCrearArchivoEnCarpetaRecibos()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var clienteRepo = new ClienteRepository(dbContext);
        var productoRepo = new ProductoRepository(dbContext);
        var ventaRepo = new VentaRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext, clienteRepo, productoRepo, ventaRepo);
        var service = new ExportService(unitOfWork);

        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            DocumentoIdentidad = "800333444-5",
            RazonSocial = "Obras y Diseños del Valle",
            Telefono = "6023344556",
            DireccionEnvio = "Av 6N # 28-10 Cali",
            Email = "contacto@obrasvalle.com"
        };
        var producto = new Producto
        {
            Id = Guid.NewGuid(),
            Nombre = "Ladrillo Estructurado Limpio",
            UnidadMedida = "Millar",
            PrecioUnitario = 1500000m,
            StockActual = 50
        };

        var venta = new Venta
        {
            Id = Guid.NewGuid(),
            ClienteId = cliente.Id,
            Cliente = cliente,
            FechaVenta = DateTime.UtcNow,
            EstadoDespacho = "En Ruta",
            Total = 3000000m // 2 millares * 1,500,000
        };

        var detalle = new VentaDetalle
        {
            Id = Guid.NewGuid(),
            VentaId = venta.Id,
            Venta = venta,
            ProductoId = producto.Id,
            Producto = producto,
            Cantidad = 2,
            PrecioAplicado = 1500000m
        };
        venta.Detalles.Add(detalle);

        await dbContext.Clientes.AddAsync(cliente);
        await dbContext.Productos.AddAsync(producto);
        await dbContext.Ventas.AddAsync(venta);
        await dbContext.SaveChangesAsync();

        var tempDir = Path.Combine(Path.GetTempPath(), "firmeza_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act
            var relativePath = await service.GuardarComprobanteReciboAsync(venta.Id, tempDir);

            // Assert
            Assert.NotNull(relativePath);
            Assert.Equal($"/recibos/recibo_{venta.Id}.pdf", relativePath);

            var fullPath = Path.Combine(tempDir, "recibos", $"recibo_{venta.Id}.pdf");
            Assert.True(File.Exists(fullPath));

            var fileBytes = await File.ReadAllBytesAsync(fullPath);
            Assert.True(fileBytes.Length > 0);
            Assert.Equal((byte)'%', fileBytes[0]);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
