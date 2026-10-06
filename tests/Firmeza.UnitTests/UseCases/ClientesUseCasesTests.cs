using Firmeza.Application.DTOS.Clientes;
using Firmeza.Application.Interfaces.Repositories;
using Firmeza.Application.UseCases.Clientes;
using Firmeza.Domain.Entities;
using Moq;
using Xunit;

namespace Firmeza.UnitTests.UseCases;

public class ClientesUseCasesTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IClienteRepository> _mockClienteRepo;

    public ClientesUseCasesTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockClienteRepo = new Mock<IClienteRepository>();
        _mockUnitOfWork.Setup(u => u.Clientes).Returns(_mockClienteRepo.Object);
    }

    [Fact]
    public async Task CrearClienteUseCase_DebeGuardarClienteCorrectamente()
    {
        // Arrange
        var useCase = new CrearClienteUseCase(_mockUnitOfWork.Object);
        var dto = new CreateClienteDto
        {
            RazonSocial = "Inversiones Caribe",
            DocumentoIdentidad = "901234567",
            Telefono = "3001234567",
            DireccionEnvio = "Calle 100 # 50-20",
            Email = "contacto@caribe.com"
        };

        _mockClienteRepo.Setup(r => r.AddAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente c, CancellationToken _) => c);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await useCase.ExecuteAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Inversiones Caribe", result.RazonSocial);
        Assert.Equal("901234567", result.DocumentoIdentidad);
        _mockClienteRepo.Verify(r => r.AddAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EliminarClienteUseCase_ConVentas_DebeLanzarExcepcion()
    {
        // Arrange
        var useCase = new EliminarClienteUseCase(_mockUnitOfWork.Object);
        var clienteId = Guid.NewGuid();
        var cliente = new Cliente
        {
            Id = clienteId,
            RazonSocial = "Cliente con Historial",
            Ventas = new List<Venta> { new() { Id = Guid.NewGuid(), Total = 500000m } }
        };

        _mockClienteRepo.Setup(r => r.GetByIdWithVentasAsync(clienteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.ExecuteAsync(clienteId));
        _mockClienteRepo.Verify(r => r.DeleteAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
