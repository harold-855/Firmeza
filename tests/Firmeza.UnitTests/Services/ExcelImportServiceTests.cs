using Firmeza.Application.DTOS.Importacion;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Firmeza.Infrastructure.Repositories;
using Firmeza.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Xunit;

namespace Firmeza.UnitTests.Services;

public class ExcelImportServiceTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task ImportarExcelDesorganizado_ColumnasMezcladas_DebeNormalizarYRelacionarEntidades()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var clienteRepo = new ClienteRepository(dbContext);
        var productoRepo = new ProductoRepository(dbContext);
        var ventaRepo = new VentaRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext, clienteRepo, productoRepo, ventaRepo);
        var service = new ExcelImportService(dbContext, unitOfWork);

        // Crear un Excel en memoria con columnas desorganizadas y mezcladas
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("DatosMezclados");

        // Fila 1: Encabezados en orden no convencional
        ws.Cells[1, 1].Value = "NIT / Cedula";
        ws.Cells[1, 2].Value = "Cantidad Vendida";
        ws.Cells[1, 3].Value = "Articulo / Producto";
        ws.Cells[1, 4].Value = "Razon Social";
        ws.Cells[1, 5].Value = "Precio Unitario";
        ws.Cells[1, 6].Value = "Telefono";
        ws.Cells[1, 7].Value = "Direccion Envio";
        ws.Cells[1, 8].Value = "Unidad Medida";
        ws.Cells[1, 9].Value = "Stock";
        ws.Cells[1, 10].Value = "Fecha Venta";
        ws.Cells[1, 11].Value = "Email";

        // Fila 2: Datos mixtos
        ws.Cells[2, 1].Value = "900.555.123-4";
        ws.Cells[2, 2].Value = 10;
        ws.Cells[2, 3].Value = "Cemento Gris Argos 50kg";
        ws.Cells[2, 4].Value = "Constructora El Roble SAS";
        ws.Cells[2, 5].Value = 35000m;
        ws.Cells[2, 6].Value = "3119876543";
        ws.Cells[2, 7].Value = "Calle 100 # 15-20";
        ws.Cells[2, 8].Value = "Bulto";
        ws.Cells[2, 9].Value = 100;
        ws.Cells[2, 10].Value = DateTime.UtcNow;
        ws.Cells[2, 11].Value = "compras@elroble.com";

        // Fila 3: Mismo cliente comprando otro producto
        ws.Cells[3, 1].Value = "900.555.123-4";
        ws.Cells[3, 2].Value = 5;
        ws.Cells[3, 3].Value = "Arena de Rio Lavada";
        ws.Cells[3, 4].Value = "Constructora El Roble SAS";
        ws.Cells[3, 5].Value = 60000m;
        ws.Cells[3, 6].Value = "3119876543";
        ws.Cells[3, 7].Value = "Calle 100 # 15-20";
        ws.Cells[3, 8].Value = "M3";
        ws.Cells[3, 9].Value = 50;
        ws.Cells[3, 10].Value = DateTime.UtcNow;
        ws.Cells[3, 11].Value = "compras@elroble.com";

        var stream = new MemoryStream(package.GetAsByteArray());

        // Act
        var result = await service.ImportarExcelDesorganizadoAsync(stream);

        // Assert
        Assert.True(result.Exitoso, string.Join("; ", result.Errores.Select(e => $"{e.Nivel}: {e.Campo} - {e.Mensaje}")));
        Assert.Equal(1, result.ClientesCreados);
        Assert.Equal(2, result.ProductosCreados);
        Assert.Equal(1, result.VentasCreadas); // Ambas filas agrupadas para el mismo cliente
        Assert.Equal(2, result.DetallesVentaCreados);

        // Verificar BD
        var cliente = await dbContext.Clientes.Include(c => c.Ventas).ThenInclude(v => v.Detalles).FirstOrDefaultAsync();
        Assert.NotNull(cliente);
        Assert.Equal("900555123-4", cliente.DocumentoIdentidad);
        Assert.Equal("Constructora El Roble SAS", cliente.RazonSocial);
        Assert.Single(cliente.Ventas);

        var venta = cliente.Ventas.First();
        Assert.Equal(2, venta.Detalles.Count);
        // Total: 10 * 35000 + 5 * 60000 = 350000 + 300000 = 650000
        Assert.Equal(650000m, venta.Total);

        // Verificar stock descontado
        var cemento = await dbContext.Productos.FirstOrDefaultAsync(p => p.Nombre == "Cemento Gris Argos 50kg");
        Assert.NotNull(cemento);
        Assert.Equal(90, cemento.StockActual); // 100 inicial - 10 vendido = 90
    }

    [Fact]
    public async Task ImportarExcelDesorganizado_ActualizacionRegistrosExistentes_DebeAplicarUpsert()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var clienteRepo = new ClienteRepository(dbContext);
        var productoRepo = new ProductoRepository(dbContext);
        var ventaRepo = new VentaRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext, clienteRepo, productoRepo, ventaRepo);
        var service = new ExcelImportService(dbContext, unitOfWork);

        // Pre-insertar cliente y producto
        var clienteExistente = new Cliente
        {
            Id = Guid.NewGuid(),
            DocumentoIdentidad = "800111222-3",
            RazonSocial = "Antigua Razon Social",
            Telefono = "1111111"
        };
        var productoExistente = new Producto
        {
            Id = Guid.NewGuid(),
            Nombre = "Varilla Corrugada 1/2",
            PrecioUnitario = 40000m,
            StockActual = 50
        };
        await dbContext.Clientes.AddAsync(clienteExistente);
        await dbContext.Productos.AddAsync(productoExistente);
        await dbContext.SaveChangesAsync();

        // Crear Excel con datos actualizados
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Actualizaciones");
        ws.Cells[1, 1].Value = "NIT";
        ws.Cells[1, 2].Value = "Razon Social";
        ws.Cells[1, 3].Value = "Telefono";
        ws.Cells[1, 4].Value = "Producto";
        ws.Cells[1, 5].Value = "Precio";
        ws.Cells[1, 6].Value = "Stock";

        ws.Cells[2, 1].Value = "800111222-3";
        ws.Cells[2, 2].Value = "Nueva Razon Social Actualizada";
        ws.Cells[2, 3].Value = "3007654321";
        ws.Cells[2, 4].Value = "Varilla Corrugada 1/2";
        ws.Cells[2, 5].Value = 49500m;
        ws.Cells[2, 6].Value = 200;

        var stream = new MemoryStream(package.GetAsByteArray());

        // Act
        var result = await service.ImportarExcelDesorganizadoAsync(stream, new ExcelImportOptionsDto { ActualizarSiExiste = true, CrearVentasSiHayDatos = false });

        // Assert
        Assert.Equal(0, result.ClientesCreados);
        Assert.Equal(1, result.ClientesActualizados);
        Assert.Equal(0, result.ProductosCreados);
        Assert.Equal(1, result.ProductosActualizados);

        var clienteDb = await dbContext.Clientes.FindAsync(clienteExistente.Id);
        Assert.NotNull(clienteDb);
        Assert.Equal("Nueva Razon Social Actualizada", clienteDb.RazonSocial);
        Assert.Equal("3007654321", clienteDb.Telefono);

        var prodDb = await dbContext.Productos.FindAsync(productoExistente.Id);
        Assert.NotNull(prodDb);
        Assert.Equal(49500m, prodDb.PrecioUnitario);
        Assert.Equal(200, prodDb.StockActual);
    }

    [Fact]
    public async Task ImportarExcelDesorganizado_DatosConInconsistencias_DebeRegistrarLogInformativoYErrores()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var clienteRepo = new ClienteRepository(dbContext);
        var productoRepo = new ProductoRepository(dbContext);
        var ventaRepo = new VentaRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext, clienteRepo, productoRepo, ventaRepo);
        var service = new ExcelImportService(dbContext, unitOfWork);

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Errores");
        ws.Cells[1, 1].Value = "Doc Identidad";
        ws.Cells[1, 2].Value = "Cliente";
        ws.Cells[1, 3].Value = "Producto";
        ws.Cells[1, 4].Value = "Precio";
        ws.Cells[1, 5].Value = "Cantidad";

        // Fila 2: Producto sin nombre
        ws.Cells[2, 1].Value = "123456";
        ws.Cells[2, 2].Value = "Juan Gomez";
        ws.Cells[2, 3].Value = ""; // Nombre vacío
        ws.Cells[2, 4].Value = 10000m;
        ws.Cells[2, 5].Value = 2;

        // Fila 3: Precio negativo
        ws.Cells[3, 1].Value = "987654";
        ws.Cells[3, 2].Value = "Pedro Perez";
        ws.Cells[3, 3].Value = "Tubo PVC";
        ws.Cells[3, 4].Value = -500m; // Precio negativo
        ws.Cells[3, 5].Value = 1;

        var stream = new MemoryStream(package.GetAsByteArray());

        // Act
        var result = await service.ImportarExcelDesorganizadoAsync(stream);

        // Assert
        Assert.NotEmpty(result.Errores);
        Assert.Contains(result.Errores, e => e.Campo == "Nombre" && e.Nivel == NivelInconsistencia.Error);
        Assert.Contains(result.Errores, e => e.Campo == "PrecioUnitario" && e.Nivel == NivelInconsistencia.Advertencia);
    }

    [Fact]
    public async Task ImportarExcelDesorganizado_EncabezadosDesplazadosYMultiplesHojas_DebeDetectarYProcesarCorrectamente()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var clienteRepo = new ClienteRepository(dbContext);
        var productoRepo = new ProductoRepository(dbContext);
        var ventaRepo = new VentaRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext, clienteRepo, productoRepo, ventaRepo);
        var service = new ExcelImportService(dbContext, unitOfWork);

        using var package = new ExcelPackage();

        // Hoja 1 con título en fila 1 y 2, encabezados reales en fila 3
        var ws1 = package.Workbook.Worksheets.Add("Clientes y Productos");
        ws1.Cells[1, 1].Value = "SISTEMA DE EXPORTACION EXTERNA";
        ws1.Cells[2, 1].Value = "GENERADO EL 2026-10-01";
        // Fila 3: Encabezados reales
        ws1.Cells[3, 1].Value = "NIT Cliente";
        ws1.Cells[3, 2].Value = "Nombre Cliente";
        ws1.Cells[3, 3].Value = "Material";
        ws1.Cells[3, 4].Value = "Precio";
        // Fila 4: Datos
        ws1.Cells[4, 1].Value = "901234567";
        ws1.Cells[4, 2].Value = "Urbanizaciones del Norte";
        ws1.Cells[4, 3].Value = "Ladrillo Toledano";
        ws1.Cells[4, 4].Value = "$ 2,500.50";

        // Hoja 2 con inventario adicional
        var ws2 = package.Workbook.Worksheets.Add("Inventario Extra");
        ws2.Cells[1, 1].Value = "Item";
        ws2.Cells[1, 2].Value = "Unidad";
        ws2.Cells[1, 3].Value = "Valor Base";
        ws2.Cells[1, 4].Value = "Existencia";
        // Fila 2: Datos
        ws2.Cells[2, 1].Value = "Malla Electrosoldada";
        ws2.Cells[2, 2].Value = "Hoja";
        ws2.Cells[2, 3].Value = "95000";
        ws2.Cells[2, 4].Value = "40";

        var stream = new MemoryStream(package.GetAsByteArray());

        // Act
        var result = await service.ImportarExcelDesorganizadoAsync(stream);

        // Assert
        Assert.True(result.Exitoso);
        Assert.Equal(2, result.TotalHojasProcesadas);
        Assert.Equal(1, result.ClientesCreados);
        Assert.Equal(2, result.ProductosCreados);

        var prod1 = await dbContext.Productos.FirstOrDefaultAsync(p => p.Nombre == "Ladrillo Toledano");
        Assert.NotNull(prod1);
        Assert.Equal(2500.50m, prod1.PrecioUnitario);

        var prod2 = await dbContext.Productos.FirstOrDefaultAsync(p => p.Nombre == "Malla Electrosoldada");
        Assert.NotNull(prod2);
        Assert.Equal(95000m, prod2.PrecioUnitario);
        Assert.Equal(40, prod2.StockActual);
    }

    [Fact]
    public async Task ImportarExcelDesorganizado_ArchivoVacio_DebeRegistrarErrorEstructural()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var clienteRepo = new ClienteRepository(dbContext);
        var productoRepo = new ProductoRepository(dbContext);
        var ventaRepo = new VentaRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext, clienteRepo, productoRepo, ventaRepo);
        var service = new ExcelImportService(dbContext, unitOfWork);

        var emptyStream = new MemoryStream();

        // Act
        var result = await service.ImportarExcelDesorganizadoAsync(emptyStream);

        // Assert
        Assert.False(result.Exitoso);
        Assert.Contains(result.Errores, e => e.Campo == "Archivo" && e.Nivel == NivelInconsistencia.Error);
    }

    [Fact]
    public void GenerarPlantillaEjemplo_DebeRetornarBytesValidosDeExcel()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var clienteRepo = new ClienteRepository(dbContext);
        var productoRepo = new ProductoRepository(dbContext);
        var ventaRepo = new VentaRepository(dbContext);
        var unitOfWork = new UnitOfWork(dbContext, clienteRepo, productoRepo, ventaRepo);
        var service = new ExcelImportService(dbContext, unitOfWork);

        // Act
        var bytes = service.GenerarPlantillaEjemplo();

        // Assert
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);

        using var stream = new MemoryStream(bytes);
        using var package = new ExcelPackage(stream);
        Assert.True(package.Workbook.Worksheets.Count >= 2);
        Assert.NotNull(package.Workbook.Worksheets["Ventas y Catalogo Mezclado"]);
    }
}
