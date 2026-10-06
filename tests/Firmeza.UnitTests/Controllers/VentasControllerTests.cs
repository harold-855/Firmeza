using Firmeza.Application.DTOS.Clientes;
using Firmeza.Application.DTOS.Productos;
using Firmeza.Application.DTOS.Ventas;
using Firmeza.Application.Interfaces;
using Firmeza.Web.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Firmeza.UnitTests.Controllers;

public class VentasControllerTests
{
    private readonly Mock<IVentaService> _mockVentaService;
    private readonly Mock<IClienteService> _mockClienteService;
    private readonly Mock<IProductoService> _mockProductoService;
    private readonly Mock<IExportService> _mockExportService;
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly Mock<ILogger<VentasController>> _mockLogger;
    private readonly VentasController _controller;

    public VentasControllerTests()
    {
        _mockVentaService = new Mock<IVentaService>();
        _mockClienteService = new Mock<IClienteService>();
        _mockProductoService = new Mock<IProductoService>();
        _mockExportService = new Mock<IExportService>();
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockLogger = new Mock<ILogger<VentasController>>();

        _mockEnvironment.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());

        _controller = new VentasController(
            _mockVentaService.Object,
            _mockClienteService.Object,
            _mockProductoService.Object,
            _mockExportService.Object,
            _mockEnvironment.Object,
            _mockLogger.Object
        );

        var httpContext = new DefaultHttpContext();
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        _controller.TempData = tempData;
    }

    [Fact]
    public async Task Index_DebeRetornarViewResult_ConMetricasYVentas()
    {
        // Arrange
        var ventas = new List<VentaDto>
        {
            new() { Id = Guid.NewGuid(), Total = 100000m, EstadoDespacho = "Pendiente" },
            new() { Id = Guid.NewGuid(), Total = 250000m, EstadoDespacho = "Entregado" }
        };

        _mockVentaService.Setup(s => s.GetAllAsync(It.IsAny<VentaFilterDto>())).ReturnsAsync(ventas);
        _mockClienteService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<ClienteDto>());

        // Act
        var result = await _controller.Index(new VentaFilterDto());

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<VentaIndexViewModel>(viewResult.Model);
        Assert.Equal(2, model.TotalVentas);
        Assert.Equal(350000m, model.MontoTotalFacturado);
        Assert.Equal(1, model.TotalPendientes);
        Assert.Equal(1, model.TotalEntregadas);
    }

    [Fact]
    public async Task Details_ConIdInexistente_DebeRedireccionarAIndex()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        _mockVentaService.Setup(s => s.GetByIdAsync(idInexistente)).ReturnsAsync((VentaDto?)null);

        // Act
        var result = await _controller.Details(idInexistente);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.NotNull(_controller.TempData["Error"]);
    }

    [Fact]
    public async Task Details_ConIdValido_DebeRetornarViewResultConVenta()
    {
        // Arrange
        var ventaId = Guid.NewGuid();
        var venta = new VentaDto
        {
            Id = ventaId,
            Total = 150000m
        };
        _mockVentaService.Setup(s => s.GetByIdAsync(venta.Id)).ReturnsAsync(venta);

        // Act
        var result = await _controller.Details(venta.Id);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<VentaDto>(viewResult.Model);
        Assert.Equal(ventaId, model.Id);
    }

    [Fact]
    public async Task Create_GET_DebeCargarClientesYProductosActivos()
    {
        // Arrange
        var clientes = new List<ClienteDto> { new() { Id = Guid.NewGuid(), RazonSocial = "Constructora ABC" } };
        var productos = new List<ProductoDto>
        {
            new() { Id = Guid.NewGuid(), Nombre = "Cemento", Activo = true },
            new() { Id = Guid.NewGuid(), Nombre = "Varilla Inactiva", Activo = false }
        };

        _mockClienteService.Setup(s => s.GetAllAsync()).ReturnsAsync(clientes);
        _mockProductoService.Setup(s => s.GetAllAsync()).ReturnsAsync(productos);

        // Act
        var result = await _controller.Create();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<VentaCreateViewModel>(viewResult.Model);
        Assert.Single(model.Clientes);
        Assert.Single(model.Productos);
        Assert.Equal("Cemento", model.Productos[0].Nombre);
    }

    [Fact]
    public async Task Create_POST_ConDetallesValidos_DebeCrearVentaYRedireccionarADetails()
    {
        // Arrange
        var dto = new CreateVentaDto
        {
            ClienteId = Guid.NewGuid(),
            Detalles = new List<CreateVentaDetalleDto>
            {
                new() { ProductoId = Guid.NewGuid(), Cantidad = 10 }
            }
        };

        var ventaCreada = new VentaDto
        {
            Id = Guid.NewGuid()
        };

        _mockVentaService.Setup(s => s.CreateAsync(dto, It.IsAny<string>()))
            .ReturnsAsync(ventaCreada);

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal(ventaCreada.Id, redirect.RouteValues?["id"]);
    }

    [Fact]
    public async Task ExportarExcel_DebeRetornarFileResult_ConMimeTypeExcel()
    {
        // Arrange
        var excelBytes = new byte[] { 0x50, 0x4B, 0x03, 0x04 };
        _mockExportService.Setup(e => e.ExportarVentasExcelAsync(It.IsAny<VentaFilterDto>()))
            .ReturnsAsync(excelBytes);

        // Act
        var result = await _controller.ExportarExcel(new VentaFilterDto());

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
        Assert.Contains(".xlsx", fileResult.FileDownloadName);
    }

    [Fact]
    public async Task ExportarPdf_DebeRetornarFileResult_ConMimeTypePdf()
    {
        // Arrange
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        _mockExportService.Setup(e => e.ExportarVentasPdfAsync(It.IsAny<VentaFilterDto>()))
            .ReturnsAsync(pdfBytes);

        // Act
        var result = await _controller.ExportarPdf(new VentaFilterDto());

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Contains(".pdf", fileResult.FileDownloadName);
    }
}
