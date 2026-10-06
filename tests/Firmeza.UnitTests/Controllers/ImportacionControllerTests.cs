using System.Text;
using Firmeza.Application.DTOS.Importacion;
using Firmeza.Application.Interfaces;
using Firmeza.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Firmeza.UnitTests.Controllers;

public class ImportacionControllerTests
{
    private readonly Mock<IExcelImportService> _mockImportService;
    private readonly Mock<ILogger<ImportacionController>> _mockLogger;
    private readonly ImportacionController _controller;

    public ImportacionControllerTests()
    {
        _mockImportService = new Mock<IExcelImportService>();
        _mockLogger = new Mock<ILogger<ImportacionController>>();

        _controller = new ImportacionController(
            _mockImportService.Object,
            _mockLogger.Object
        );

        var httpContext = new DefaultHttpContext();
        var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        _controller.TempData = tempData;
    }

    [Fact]
    public void Index_DebeRetornarViewResult_ConModeloInicial()
    {
        // Act
        var result = _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<ExcelImportViewModel>(viewResult.Model);
    }

    [Fact]
    public async Task Procesar_SinArchivo_DebeRetornarErrorEnModelState()
    {
        // Act
        var result = await _controller.Procesar(null, new ExcelImportOptionsDto(), CancellationToken.None);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
        Assert.True(_controller.ModelState.ContainsKey("ArchivoExcel"));
    }

    [Fact]
    public async Task Procesar_ConExtensionInvalida_DebeRetornarError()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("archivo.txt");
        fileMock.Setup(f => f.Length).Returns(100);

        // Act
        var result = await _controller.Procesar(fileMock.Object, new ExcelImportOptionsDto(), CancellationToken.None);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(_controller.ModelState.IsValid);
        Assert.True(_controller.ModelState.ContainsKey("ArchivoExcel"));
    }

    [Fact]
    public async Task Procesar_ConArchivoValido_DebeEjecutarServicioYRetornarResultado()
    {
        // Arrange
        var content = "Fake Excel Bytes";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("datos_desorganizados.xlsx");
        fileMock.Setup(f => f.Length).Returns(stream.Length);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

        var importResult = new ExcelImportResultDto
        {
            ClientesCreados = 2,
            ProductosCreados = 5,
            VentasCreadas = 3
        };

        _mockImportService.Setup(s => s.ImportarExcelDesorganizadoAsync(
            It.IsAny<Stream>(),
            It.IsAny<ExcelImportOptionsDto>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(importResult);

        // Act
        var result = await _controller.Procesar(fileMock.Object, new ExcelImportOptionsDto(), CancellationToken.None);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ExcelImportViewModel>(viewResult.Model);
        Assert.NotNull(model.Resultado);
        Assert.True(model.Resultado.Exitoso);
        Assert.Equal(5, model.Resultado.ProductosCreados);
    }

    [Fact]
    public void DescargarPlantilla_DebeRetornarArchivoExcel()
    {
        // Arrange
        var plantillaBytes = new byte[] { 0x50, 0x4B, 0x03, 0x04 };
        _mockImportService.Setup(s => s.GenerarPlantillaEjemplo())
            .Returns(plantillaBytes);

        // Act
        var result = _controller.DescargarPlantilla();

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
        Assert.Contains(".xlsx", fileResult.FileDownloadName);
    }
}
