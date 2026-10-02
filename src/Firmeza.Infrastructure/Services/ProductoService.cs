using Firmeza.Application.DTOS.Productos;
using Firmeza.Application.Interfaces;
using Firmeza.Application.Interfaces.Repositories;
using Firmeza.Domain.Entities;

namespace Firmeza.Infrastructure.Services;

public class ProductoService(IUnitOfWork unitOfWork) : IProductoService
{
    public async Task<IEnumerable<ProductoDto>> GetAllAsync(ProductoFilterDto? filter = null)
    {
        var productos = await unitOfWork.Productos.GetAllWithDetallesAsync(filter);

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

    public async Task<ProductoDto?> GetByIdAsync(Guid id)
    {
        var p = await unitOfWork.Productos.GetByIdWithDetallesAsync(id);
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

    public async Task<ProductoDto> CreateAsync(CreateProductoDto dto)
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

        await unitOfWork.Productos.AddAsync(producto);
        await unitOfWork.SaveChangesAsync();

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

    public async Task<bool> UpdateAsync(Guid id, UpdateProductoDto dto)
    {
        var producto = await unitOfWork.Productos.GetByIdAsync(id);
        if (producto == null)
            return false;

        producto.Nombre = dto.Nombre.Trim();
        producto.Descripcion = dto.Descripcion.Trim();
        producto.UnidadMedida = dto.UnidadMedida.Trim();
        producto.PrecioUnitario = dto.PrecioUnitario;
        producto.StockActual = dto.StockActual;
        producto.Activo = dto.Activo;

        await unitOfWork.Productos.UpdateAsync(producto);
        await unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var producto = await unitOfWork.Productos.GetByIdWithDetallesAsync(id);
        if (producto == null)
            return false;

        // Si tiene ventas asociadas, desactivamos lógicamente para proteger integridad histórica
        if (producto.DetallesVenta.Count > 0)
        {
            producto.Activo = false;
            await unitOfWork.Productos.UpdateAsync(producto);
        }
        else
        {
            await unitOfWork.Productos.DeleteAsync(producto);
        }

        await unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await unitOfWork.Productos.ExistsAsync(id);
    }

    public async Task<IEnumerable<string>> GetUnidadesMedidaAsync()
    {
        return await unitOfWork.Productos.GetUnidadesMedidaAsync();
    }
}
