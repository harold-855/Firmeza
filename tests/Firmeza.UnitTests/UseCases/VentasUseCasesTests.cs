using Firmeza.Application.DTOS.Ventas;
using Firmeza.Application.Interfaces;
using Firmeza.Application.Interfaces.Repositories;
using Firmeza.Application.UseCases.Ventas;
using Firmeza.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Firmeza.UnitTests.UseCases;

public class VentasUseCasesTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IClienteRepository> _mockClienteRepo;
    private readonly Mock<IProductoRepository> _mockProductoRepo;
    private readonly Mock<IVentaRepository> _mockVentaRepo;
    private readonly Mock<IExportService> _mockExportService;

    public VentasUseCasesTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockClienteRepo = new Mock<IClienteRepository>();
        _mockProductoRepo = new Mock<IProductoRepository>();
        _mockVentaRepo = new Mock<IVentaRepository>();
        _mockExportService = new Mock<IExportService>();

        _mockUnitOfWork.Setup(u => u.Clientes).Returns(_mockClienteRepo.Object);
        _mockUnitOfWork.Setup(u => u.Productos).Returns(_mockProductoRepo.Object);
        _mockUnitOfWork.Setup(u => u.Ventas).Returns(_mockVentaRepo.Object);
    }

    [Fact]
    public async Task ActualizarEstadoDespachoUseCase_VentaExistente_DebeActualizarYGuardar()
    {
        // Arrange
        var useCase = new ActualizarEstadoDespachoUseCase(_mockUnitOfWork.Object);
        var ventaId = Guid.NewGuid();
        var venta = new Venta
        {
            Id = ventaId,
            EstadoDespacho = "Pendiente",
            Total = 100000m
        };

        _mockVentaRepo.Setup(r => r.GetByIdAsync(ventaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(venta);
        _mockVentaRepo.Setup(r => r.UpdateAsync(venta, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await useCase.ExecuteAsync(ventaId, "En Ruta");

        // Assert
        Assert.True(result);
        Assert.Equal("En Ruta", venta.EstadoDespacho);
        _mockVentaRepo.Verify(r => r.UpdateAsync(venta, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
