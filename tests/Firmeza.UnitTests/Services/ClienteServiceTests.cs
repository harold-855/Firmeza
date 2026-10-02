using Firmeza.Application.DTOS.Clientes;
using Firmeza.Application.Interfaces.Repositories;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Services;
using Moq;
using Xunit;

namespace Firmeza.UnitTests.Services;

public class ClienteServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IClienteRepository> _mockClienteRepo;
    private readonly ClienteService _clienteService;

    public ClienteServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockClienteRepo = new Mock<IClienteRepository>();

        _mockUnitOfWork.Setup(u => u.Clientes).Returns(_mockClienteRepo.Object);
        _clienteService = new ClienteService(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task DeleteAsync_ClienteConComprasAsociadas_DebeLanzarInvalidOperationException()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var clienteConCompras = new Cliente
        {
            Id = clienteId,
            RazonSocial = "Constructora del Valle S.A.S.",
            DocumentoIdentidad = "900987654-1",
            Ventas = new List<Venta>
            {
                new() { Id = Guid.NewGuid(), Total = 1500000m, EstadoDespacho = "Entregado" }
            }
        };

        _mockClienteRepo
            .Setup(r => r.GetByIdWithVentasAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteConCompras);

        // Act & Assert (Verificar que se dispare la excepción protegiendo la integridad)
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _clienteService.DeleteAsync(clienteId));

        Assert.Contains("cuenta con órdenes de venta", exception.Message);
        _mockClienteRepo.Verify(r => r.DeleteAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ClienteSinCompras_DebeEliminarYGuardarCambios()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var clienteSinCompras = new Cliente
        {
            Id = clienteId,
            RazonSocial = "Cliente Sin Pedidos",
            DocumentoIdentidad = "1020304050",
            Ventas = new List<Venta>()
        };

        _mockClienteRepo
            .Setup(r => r.GetByIdWithVentasAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(clienteSinCompras);

        _mockClienteRepo
            .Setup(r => r.DeleteAsync(clienteSinCompras, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var resultado = await _clienteService.DeleteAsync(clienteId);

        // Assert
        Assert.True(resultado);
        _mockClienteRepo.Verify(r => r.DeleteAsync(clienteSinCompras, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
