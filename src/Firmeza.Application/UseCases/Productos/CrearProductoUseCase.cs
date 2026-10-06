using Firmeza.Application.DTOS.Productos;
using Firmeza.Application.Interfaces.Repositories;
using Firmeza.Domain.Entities;

namespace Firmeza.Application.UseCases.Productos;

public class CrearProductoUseCase(IUnitOfWork unitOfWork)
{
    public async Task<ProductoDto> ExecuteAsync(CreateProductoDto dto, CancellationToken cancellationToken = default)
    {
        var producto = new Producto
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion.Trim(),
            UnidadMedida = dto.UnidadMedida.Trim(),
            PrecioUnitario = dto.PrecioUnitario,
            StockActual = dto.StockActual,
            Activo = dto.Activo
        };

        await unitOfWork.Productos.AddAsync(producto, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProductoDto
        {
            Id = producto.Id,
            Nombre = producto.Nombre,
            Descripcion = producto.Descripcion,
            UnidadMedida = producto.UnidadMedida,
            PrecioUnitario = producto.PrecioUnitario,
            StockActual = producto.StockActual,
            Activo = producto.Activo,
            TotalVentasAsociadas = 0
        };
    }
}
