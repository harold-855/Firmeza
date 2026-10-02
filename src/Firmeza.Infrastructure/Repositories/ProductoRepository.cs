using Firmeza.Application.DTOS.Productos;
using Firmeza.Application.Interfaces.Repositories;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Repositories;

public class ProductoRepository(ApplicationDbContext context) 
    : BaseRepository<Producto>(context), IProductoRepository
{
    public async Task<IEnumerable<Producto>> GetAllWithDetallesAsync(ProductoFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Productos
            .Include(p => p.DetallesVenta)
            .AsNoTracking()
            .AsQueryable();

        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                query = query.Where(p =>
                    p.Nombre.ToLower().Contains(term) ||
                    p.Descripcion.ToLower().Contains(term) ||
                    p.UnidadMedida.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(filter.UnidadMedida))
            {
                query = query.Where(p => p.UnidadMedida == filter.UnidadMedida);
            }

            if (filter.SoloActivos.HasValue)
            {
                query = query.Where(p => p.Activo == filter.SoloActivos.Value);
            }

            if (filter.SoloBajoStock == true)
            {
                var umbral = filter.UmbralBajoStock > 0 ? filter.UmbralBajoStock : 50;
                query = query.Where(p => p.StockActual < umbral);
            }

            if (filter.PrecioMin.HasValue)
            {
                query = query.Where(p => p.PrecioUnitario >= filter.PrecioMin.Value);
            }

            if (filter.PrecioMax.HasValue)
            {
                query = query.Where(p => p.PrecioUnitario <= filter.PrecioMax.Value);
            }

            query = filter.OrderBy switch
            {
                "nombre_desc" => query.OrderByDescending(p => p.Nombre),
                "precio_asc" => query.OrderBy(p => p.PrecioUnitario),
                "precio_desc" => query.OrderByDescending(p => p.PrecioUnitario),
                "stock_asc" => query.OrderBy(p => p.StockActual),
                "stock_desc" => query.OrderByDescending(p => p.StockActual),
                _ => query.OrderBy(p => p.Nombre)
            };
        }
        else
        {
            query = query.OrderBy(p => p.Nombre);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Producto?> GetByIdWithDetallesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Productos
            .Include(p => p.DetallesVenta)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<string>> GetUnidadesMedidaAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Productos
            .Select(p => p.UnidadMedida)
            .Distinct()
            .OrderBy(u => u)
            .ToListAsync(cancellationToken);
    }
}
