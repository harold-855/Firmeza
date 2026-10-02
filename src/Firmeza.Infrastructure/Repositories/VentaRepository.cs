using Firmeza.Application.Interfaces.Repositories;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Repositories;

public class VentaRepository(ApplicationDbContext context) 
    : BaseRepository<Venta>(context), IVentaRepository
{
    public async Task<IEnumerable<Venta>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .OrderByDescending(v => v.FechaVenta)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Venta?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Venta>> GetUltimasVentasAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
            .OrderByDescending(v => v.FechaVenta)
            .Take(count)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
