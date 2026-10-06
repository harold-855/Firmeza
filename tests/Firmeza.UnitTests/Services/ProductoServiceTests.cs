using Firmeza.Application.DTOS.Productos;
using Firmeza.Application.Interfaces.Repositories;
using Firmeza.Application.Services;
using Firmeza.Application.UseCases.Productos;
using Firmeza.Domain.Entities;
using Moq;
using Xunit;

namespace Firmeza.UnitTests.Services;

public class ProductoServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IProductoRepository> _mockProductoRepo;
    private readonly ProductoService _productoService;

    public ProductoServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockProductoRepo = new Mock<IProductoRepository>();

        _mockUnitOfWork.Setup(u => u.Productos).Returns(_mockProductoRepo.Object);
        var obtenerUc = new ObtenerProductosUseCase(_mockUnitOfWork.Object);
        var crearUc = new CrearProductoUseCase(_mockUnitOfWork.Object);
        var actualizarUc = new ActualizarProductoUseCase(_mockUnitOfWork.Object);
        var eliminarUc = new EliminarProductoUseCase(_mockUnitOfWork.Object);

        _productoService = new ProductoService(obtenerUc, crearUc, actualizarUc, eliminarUc);
    }

    [Fact]
    public async Task DeleteAsync_ProductoConVentasAsociadas_DebeRealizarSoftDeleteMarcandoInactivo()
    {
        // Arrange
        var productoId = Guid.NewGuid();
        var productoConVentas = new Producto
        {
            Id = productoId,
            Nombre = "Cemento Gris",
            Activo = true,
            DetallesVenta = new List<VentaDetalle>
            {
                new() { Id = Guid.NewGuid(), Cantidad = 10, PrecioAplicado = 32000m }
            }
        };

        _mockProductoRepo
            .Setup(r => r.GetByIdWithDetallesAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(productoConVentas);

        _mockProductoRepo
            .Setup(r => r.UpdateAsync(productoConVentas, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var resultado = await _productoService.DeleteAsync(productoId);

        // Assert
        Assert.True(resultado);
        Assert.False(productoConVentas.Activo); // Soft Delete: se desactiva
        _mockProductoRepo.Verify(r => r.UpdateAsync(productoConVentas, It.IsAny<CancellationToken>()), Times.Once);
        _mockProductoRepo.Verify(r => r.DeleteAsync(It.IsAny<Producto>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DatosValidos_DebeCrearProductoYGuardarEnRepositorio()
    {
        // Arrange
        var createDto = new CreateProductoDto
        {
            Nombre = "Arena Lavada de Río (M3)",
            Descripcion = "Arena fina para mezclas y mampostería",
            UnidadMedida = "M3",
            PrecioUnitario = 85000m,
            StockActual = 40,
            Activo = true
        };

        _mockProductoRepo
            .Setup(r => r.AddAsync(It.IsAny<Producto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Producto p, CancellationToken _) => p);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var resultado = await _productoService.CreateAsync(createDto);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(createDto.Nombre, resultado.Nombre);
        Assert.Equal(createDto.PrecioUnitario, resultado.PrecioUnitario);
        Assert.Equal(createDto.StockActual, resultado.StockActual);
        Assert.True(resultado.Activo);
        _mockProductoRepo.Verify(r => r.AddAsync(It.IsAny<Producto>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
