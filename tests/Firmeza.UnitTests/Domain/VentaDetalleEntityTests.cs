using Firmeza.Domain.Entities;
using Xunit;

namespace Firmeza.UnitTests.Domain;

public class VentaDetalleEntityTests
{
    [Fact]
    public void VentaDetalle_CalculoSubtotal_DebeMultiplicarCantidadPorPrecioAplicado()
    {
        // Arrange
        var cantidad = 5;
        var precioUnitarioCongelado = 45000m; // Varilla 1/2"

        // Act
        var detalle = new VentaDetalle
        {
            Id = Guid.NewGuid(),
            Cantidad = cantidad,
            PrecioAplicado = precioUnitarioCongelado
        };

        // Assert
        Assert.Equal(225000m, detalle.Subtotal);
        Assert.Equal(5, detalle.Cantidad);
        Assert.Equal(45000m, detalle.PrecioAplicado);
    }
}
