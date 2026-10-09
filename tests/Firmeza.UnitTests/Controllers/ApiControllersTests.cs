using AutoMapper;
using Firmeza.Api.Controllers;
using Firmeza.Application.DTOS.Clientes;
using Firmeza.Application.DTOS.Productos;
using Firmeza.Application.DTOS.Ventas;
using Firmeza.Application.Interfaces;
using Firmeza.Application.Interfaces.Repositories;
using Firmeza.Application.Mappings;
using Firmeza.Application.UseCases.Clientes;
using Firmeza.Application.UseCases.Productos;
using Firmeza.Application.UseCases.Ventas;
using Firmeza.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Firmeza.UnitTests.Controllers;

public class ApiControllersTests
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IExportService> _mockExport;
    private readonly Mock<IWebHostEnvironment> _mockEnv;
    private readonly Mock<ILogger<CrearVentaUseCase>> _mockLoggerVenta;
    private readonly IMapper _mapper;

    public ApiControllersTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockExport = new Mock<IExportService>();
        _mockEnv = new Mock<IWebHostEnvironment>();
        _mockLoggerVenta = new Mock<ILogger<CrearVentaUseCase>>();
        _mockEnv.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<ProductoMappingProfile>();
            cfg.AddProfile<ClienteMappingProfile>();
            cfg.AddProfile<VentaMappingProfile>();
        });
        var sp = services.BuildServiceProvider();
        _mapper = sp.GetRequiredService<IMapper>();
    }

    [Fact]
    public async Task ProductosApiController_GetAll_DebeRetornarOkConListado()
    {
        // Arrange
        var productos = new List<Producto>
        {
            new Producto { Id = Guid.NewGuid(), Nombre = "Cemento", UnidadMedida = "Bolsa", PrecioUnitario = 30000m, StockActual = 100, Activo = true }
        };
        _mockUow.Setup(u => u.Productos.GetAllWithDetallesAsync(It.IsAny<ProductoFilterDto?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(productos);

        var obtenerUseCase = new ObtenerProductosUseCase(_mockUow.Object, _mapper);
        var crearUseCase = new CrearProductoUseCase(_mockUow.Object, _mapper);
        var actualizarUseCase = new ActualizarProductoUseCase(_mockUow.Object);
        var eliminarUseCase = new EliminarProductoUseCase(_mockUow.Object);

        var controller = new ProductosApiController(obtenerUseCase, crearUseCase, actualizarUseCase, eliminarUseCase);

        // Act
        var result = await controller.GetAll(new ProductoFilterDto(), CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var lista = Assert.IsAssignableFrom<IEnumerable<ProductoDto>>(okResult.Value);
        Assert.Single(lista);
    }

    [Fact]
    public async Task ProductosApiController_Create_DebeRetornarCreatedAtAction()
    {
        // Arrange
        _mockUow.Setup(u => u.Productos.AddAsync(It.IsAny<Producto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Producto p, CancellationToken _) => p);
        _mockUow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var obtenerUseCase = new ObtenerProductosUseCase(_mockUow.Object, _mapper);
        var crearUseCase = new CrearProductoUseCase(_mockUow.Object, _mapper);
        var actualizarUseCase = new ActualizarProductoUseCase(_mockUow.Object);
        var eliminarUseCase = new EliminarProductoUseCase(_mockUow.Object);

        var controller = new ProductosApiController(obtenerUseCase, crearUseCase, actualizarUseCase, eliminarUseCase);
        var dto = new CreateProductoDto { Nombre = "Arena", UnidadMedida = "M3", PrecioUnitario = 50000m, StockActual = 20, Activo = true };

        // Act
        var result = await controller.Create(dto, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var productoDto = Assert.IsType<ProductoDto>(createdResult.Value);
        Assert.Equal("Arena", productoDto.Nombre);
    }

    [Fact]
    public async Task ClientesApiController_GetAll_DebeRetornarOkConListado()
    {
        // Arrange
        var clientes = new List<Cliente>
        {
            new Cliente { Id = Guid.NewGuid(), DocumentoIdentidad = "900111222", RazonSocial = "Constructora Alpha" }
        };
        _mockUow.Setup(u => u.Clientes.GetAllWithVentasAsync(It.IsAny<ClienteFilterDto?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientes);

        var obtenerUseCase = new ObtenerClientesUseCase(_mockUow.Object, _mapper);
        var crearUseCase = new CrearClienteUseCase(_mockUow.Object, _mapper);
        var actualizarUseCase = new ActualizarClienteUseCase(_mockUow.Object);
        var eliminarUseCase = new EliminarClienteUseCase(_mockUow.Object);

        var controller = new ClientesApiController(obtenerUseCase, crearUseCase, actualizarUseCase, eliminarUseCase);

        // Act
        var result = await controller.GetAll(new ClienteFilterDto(), CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var lista = Assert.IsAssignableFrom<IEnumerable<ClienteDto>>(okResult.Value);
        Assert.Single(lista);
    }

    [Fact]
    public async Task ClientesApiController_Create_DebeRetornarCreatedAtAction()
    {
        // Arrange
        _mockUow.Setup(u => u.Clientes.DocumentoExistsAsync("900333444", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _mockUow.Setup(u => u.Clientes.AddAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente c, CancellationToken _) => c);
        _mockUow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var obtenerUseCase = new ObtenerClientesUseCase(_mockUow.Object, _mapper);
        var crearUseCase = new CrearClienteUseCase(_mockUow.Object, _mapper);
        var actualizarUseCase = new ActualizarClienteUseCase(_mockUow.Object);
        var eliminarUseCase = new EliminarClienteUseCase(_mockUow.Object);

        var controller = new ClientesApiController(obtenerUseCase, crearUseCase, actualizarUseCase, eliminarUseCase);
        var dto = new CreateClienteDto { DocumentoIdentidad = "900333444", RazonSocial = "Beta SAS", Telefono = "3111111111", Email = "beta@test.com", DireccionEnvio = "Calle 1" };

        // Act
        var result = await controller.Create(dto, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var clienteDto = Assert.IsType<ClienteDto>(createdResult.Value);
        Assert.Equal("Beta SAS", clienteDto.RazonSocial);
    }

    [Fact]
    public async Task VentasApiController_GetAll_DebeRetornarOkConListado()
    {
        // Arrange
        var cliente = new Cliente { Id = Guid.NewGuid(), RazonSocial = "Cliente 1", DocumentoIdentidad = "12345" };
        var ventas = new List<Venta>
        {
            new Venta { Id = Guid.NewGuid(), ClienteId = cliente.Id, Cliente = cliente, Total = 100000m, EstadoDespacho = "Entregado" }
        };
        _mockUow.Setup(u => u.Ventas.GetAllWithDetailsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ventas);

        var obtenerUseCase = new ObtenerVentasUseCase(_mockUow.Object, _mapper);
        var crearUseCase = new CrearVentaUseCase(_mockUow.Object, _mockExport.Object, _mockLoggerVenta.Object, _mapper);
        var actualizarEstadoUseCase = new ActualizarEstadoDespachoUseCase(_mockUow.Object);

        var controller = new VentasApiController(obtenerUseCase, crearUseCase, actualizarEstadoUseCase, _mockExport.Object, _mockEnv.Object);

        // Act
        var result = await controller.GetAll(new VentaFilterDto(), CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var lista = Assert.IsAssignableFrom<IEnumerable<VentaDto>>(okResult.Value);
        Assert.Single(lista);
    }
}
