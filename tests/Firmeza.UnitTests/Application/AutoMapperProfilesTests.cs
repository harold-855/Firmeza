using AutoMapper;
using Firmeza.Application.DTOS.Clientes;
using Firmeza.Application.DTOS.Productos;
using Firmeza.Application.DTOS.Ventas;
using Firmeza.Application.Mappings;
using Firmeza.Domain.Entities;
using Xunit;

namespace Firmeza.UnitTests.Application;

public class AutoMapperProfilesTests
{
    private readonly IMapper _mapper;

    public AutoMapperProfilesTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ProductoMappingProfile>();
            cfg.AddProfile<ClienteMappingProfile>();
            cfg.AddProfile<VentaMappingProfile>();
        });

        config.AssertConfigurationIsValid();
        _mapper = config.CreateMapper();
    }

    [Fact]
    public void Configuration_DebeSerValida()
    {
        // AssertConfigurationIsValid fue llamado en el constructor sin lanzar excepciones
        Assert.NotNull(_mapper);
    }

    [Fact]
    public void Mapear_Producto_A_ProductoDto_DebeMapearPropiedadesCorrectamente()
    {
        // Arrange
        var producto = new Producto
        {
            Id = Guid.NewGuid(),
            Nombre = "Cemento Gris",
            Descripcion = "Bolsa 50kg",
            UnidadMedida = "Bolsa",
            PrecioUnitario = 32000m,
            StockActual = 120,
            Activo = true
        };

        // Act
        var dto = _mapper.Map<ProductoDto>(producto);

        // Assert
        Assert.Equal(producto.Id, dto.Id);
        Assert.Equal(producto.Nombre, dto.Nombre);
        Assert.Equal(producto.Descripcion, dto.Descripcion);
        Assert.Equal(producto.UnidadMedida, dto.UnidadMedida);
        Assert.Equal(producto.PrecioUnitario, dto.PrecioUnitario);
        Assert.Equal(producto.StockActual, dto.StockActual);
        Assert.Equal(producto.Activo, dto.Activo);
    }

    [Fact]
    public void Mapear_CreateProductoDto_A_Producto_DebeMapearCorrectamente()
    {
        // Arrange
        var dto = new CreateProductoDto
        {
            Nombre = "Varilla 1/2",
            Descripcion = "Varilla corrugada",
            UnidadMedida = "Unidad",
            PrecioUnitario = 45000m,
            StockActual = 50,
            Activo = true
        };

        // Act
        var producto = _mapper.Map<Producto>(dto);

        // Assert
        Assert.Equal(dto.Nombre, producto.Nombre);
        Assert.Equal(dto.Descripcion, producto.Descripcion);
        Assert.Equal(dto.UnidadMedida, producto.UnidadMedida);
        Assert.Equal(dto.PrecioUnitario, producto.PrecioUnitario);
        Assert.Equal(dto.StockActual, producto.StockActual);
        Assert.Equal(dto.Activo, producto.Activo);
    }

    [Fact]
    public void Mapear_Cliente_A_ClienteDto_DebeMapearTotalComprasYPropiedades()
    {
        // Arrange
        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            DocumentoIdentidad = "900123456-1",
            RazonSocial = "Constructora Firmeza SAS",
            Telefono = "3001234567",
            Email = "contacto@firmeza.com",
            DireccionEnvio = "Calle 100 # 15-20",
            Ventas = new List<Venta>
            {
                new Venta { Total = 150000m },
                new Venta { Total = 250000m }
            }
        };

        // Act
        var dto = _mapper.Map<ClienteDto>(cliente);

        // Assert
        Assert.Equal(cliente.Id, dto.Id);
        Assert.Equal(cliente.DocumentoIdentidad, dto.DocumentoIdentidad);
        Assert.Equal(cliente.RazonSocial, dto.RazonSocial);
        Assert.Equal(cliente.Telefono, dto.Telefono);
        Assert.Equal(cliente.Email, dto.Email);
        Assert.Equal(cliente.DireccionEnvio, dto.DireccionEnvio);
        Assert.Equal(2, dto.TotalCompras);
        Assert.Equal(400000m, dto.MontoTotalComprado);
    }

    [Fact]
    public void Mapear_Venta_A_VentaDto_DebeMapearDetallesYCliente()
    {
        // Arrange
        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            DocumentoIdentidad = "800999888-0",
            RazonSocial = "Obras y Acabados"
        };

        var producto = new Producto
        {
            Id = Guid.NewGuid(),
            Nombre = "Arena de Río",
            UnidadMedida = "M3"
        };

        var venta = new Venta
        {
            Id = Guid.NewGuid(),
            ClienteId = cliente.Id,
            Cliente = cliente,
            FechaVenta = DateTime.UtcNow,
            Total = 95000m,
            EstadoDespacho = "Entregado",
            Detalles = new List<VentaDetalle>
            {
                new VentaDetalle
                {
                    Id = Guid.NewGuid(),
                    ProductoId = producto.Id,
                    Producto = producto,
                    Cantidad = 2,
                    PrecioAplicado = 47500m
                }
            }
        };

        // Act
        var dto = _mapper.Map<VentaDto>(venta);

        // Assert
        Assert.Equal(venta.Id, dto.Id);
        Assert.Equal("Obras y Acabados", dto.ClienteRazonSocial);
        Assert.Equal("800999888-0", dto.ClienteDocumento);
        Assert.Equal(95000m, dto.Total);
        Assert.Equal("Entregado", dto.EstadoDespacho);
        Assert.Single(dto.Detalles);
        Assert.Equal("Arena de Río", dto.Detalles[0].ProductoNombre);
        Assert.Equal("M3", dto.Detalles[0].UnidadMedida);
        Assert.Equal(2, dto.Detalles[0].Cantidad);
        Assert.Equal(47500m, dto.Detalles[0].PrecioAplicado);
        Assert.Equal(95000m, dto.Detalles[0].Subtotal);
    }
}
