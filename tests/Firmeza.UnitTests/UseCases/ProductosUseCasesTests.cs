using Firmeza.Application.DTOS.Productos;
using Firmeza.Application.Interfaces.Repositories;
using Firmeza.Application.UseCases.Productos;
using Firmeza.Domain.Entities;
using Moq;
using Xunit;

namespace Firmeza.UnitTests.UseCases;

public class ProductosUseCasesTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IProductoRepository> _mockProductoRepo;

    public ProductosUseCasesTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockProductoRepo = new Mock<IProductoRepository>();
        _mockUnitOfWork.Setup(u => u.Productos).Returns(_mockProductoRepo.Object);
    }

    [Fact]
    public async Task CrearProductoUseCase_DebeGuardarYRetornarDto()
    {
        // Arrange
        var useCase = new CrearProductoUseCase(_mockUnitOfWork.Object);
        var dto = new CreateProductoDto
        {
            Nombre = "Ladrillo Estructurado",
            Descripcion = "Ladrillo de arcilla",
            UnidadMedida = "UND",
            PrecioUnitario = 1500m,
            StockActual = 500,
            Activo = true
        };

        _mockProductoRepo.Setup(r => r.AddAsync(It.IsAny<Producto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Producto p, CancellationToken _) => p);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await useCase.ExecuteAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Ladrillo Estructurado", result.Nombre);
        Assert.Equal(1500m, result.PrecioUnitario);
        _mockProductoRepo.Verify(r => r.AddAsync(It.IsAny<Producto>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EliminarProductoUseCase_ConVentasAsociadas_DebeDesactivarLogicamente()
    {
        // Arrange
        var useCase = new EliminarProductoUseCase(_mockUnitOfWork.Object);
        var productoId = Guid.NewGuid();
        var producto = new Producto
        {
            Id = productoId,
            Nombre = "Cemento",
            Activo = true,
            DetallesVenta = new List<VentaDetalle> { new() { Id = Guid.NewGuid() } }
        };

        _mockProductoRepo.Setup(r => r.GetByIdWithDetallesAsync(productoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producto);
        _mockProductoRepo.Setup(r => r.UpdateAsync(producto, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await useCase.ExecuteAsync(productoId);

        // Assert
        Assert.True(result);
        Assert.False(producto.Activo);
        _mockProductoRepo.Verify(r => r.UpdateAsync(producto, It.IsAny<CancellationToken>()), Times.Once);
        _mockProductoRepo.Verify(r => r.DeleteAsync(It.IsAny<Producto>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
