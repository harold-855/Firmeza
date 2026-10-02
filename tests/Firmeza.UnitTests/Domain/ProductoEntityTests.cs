using Firmeza.Domain.Entities;
using Xunit;

namespace Firmeza.UnitTests.Domain;

public class ProductoEntityTests
{
    [Fact]
    public void Producto_CreacionValida_DebeInicializarCorrectamentePropiedades()
    {
        // Arrange (Preparar el escenario)
        var productoId = Guid.NewGuid();
        var nombre = "Cemento Gris Tipo 1 x 50kg";
        var descripcion = "Bolsa de cemento de alta resistencia estructural";
        var unidadMedida = "Bolsa";
        var precioUnitario = 32000m;
        var stockInicial = 100;

        // Act (Ejecutar la acción a probar)
        var producto = new Producto
        {
            Id = productoId,
            Nombre = nombre,
            Descripcion = descripcion,
            UnidadMedida = unidadMedida,
            PrecioUnitario = precioUnitario,
            StockActual = stockInicial,
            Activo = true
        };

        // Assert (Verificar los resultados esperados)
        Assert.Equal(productoId, producto.Id);
        Assert.Equal(nombre, producto.Nombre);
        Assert.Equal(unidadMedida, producto.UnidadMedida);
        Assert.Equal(32000m, producto.PrecioUnitario);
        Assert.Equal(100, producto.StockActual);
        Assert.True(producto.Activo);
        Assert.NotNull(producto.DetallesVenta);
        Assert.Empty(producto.DetallesVenta);
    }

    [Theory]
    [InlineData(10, true)]
    [InlineData(49, true)]
    [InlineData(50, false)]
    [InlineData(100, false)]
    public void Producto_EvaluacionStockBajo_DebeDeterminarSiRequiereAlerta(int stock, bool alertaEsperada)
    {
        // Arrange
        var producto = new Producto
        {
            Nombre = "Ladrillo Estructurado",
            StockActual = stock,
            Activo = true
        };

        // Act: El umbral crítico de stock en Firmeza es < 50 unidades
        var alertaBajoStock = producto.StockActual < 50;

        // Assert
        Assert.Equal(alertaEsperada, alertaBajoStock);
    }
}
