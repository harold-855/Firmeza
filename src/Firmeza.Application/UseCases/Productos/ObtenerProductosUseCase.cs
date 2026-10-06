using Firmeza.Application.DTOS.Productos;
using Firmeza.Application.Interfaces.Repositories;

namespace Firmeza.Application.UseCases.Productos;

public class ObtenerProductosUseCase(IUnitOfWork unitOfWork)
{
    public async Task<IEnumerable<ProductoDto>> ExecuteGetAllAsync(ProductoFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        var productos = await unitOfWork.Productos.GetAllWithDetallesAsync(filter, cancellationToken);

        return productos.Select(p => new ProductoDto
        {
            Id = p.Id,
            Nombre = p.Nombre,
            Descripcion = p.Descripcion,
            UnidadMedida = p.UnidadMedida,
            PrecioUnitario = p.PrecioUnitario,
            StockActual = p.StockActual,
            Activo = p.Activo,
            TotalVentasAsociadas = p.DetallesVenta.Count
        }).ToList();
    }

    public async Task<ProductoDto?> ExecuteGetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var p = await unitOfWork.Productos.GetByIdWithDetallesAsync(id, cancellationToken);
        if (p == null) return null;

        return new ProductoDto
        {
            Id = p.Id,
            Nombre = p.Nombre,
            Descripcion = p.Descripcion,
            UnidadMedida = p.UnidadMedida,
            PrecioUnitario = p.PrecioUnitario,
            StockActual = p.StockActual,
            Activo = p.Activo,
            TotalVentasAsociadas = p.DetallesVenta.Count
        };
    }

    public async Task<IEnumerable<string>> ExecuteGetUnidadesMedidaAsync(CancellationToken cancellationToken = default)
    {
        return await unitOfWork.Productos.GetUnidadesMedidaAsync(cancellationToken);
    }

    public async Task<bool> ExecuteExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await unitOfWork.Productos.ExistsAsync(id, cancellationToken);
    }
}
