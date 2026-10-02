using Firmeza.Application.DTOS.Clientes;
using Firmeza.Application.Interfaces.Repositories;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Repositories;

public class ClienteRepository(ApplicationDbContext context) 
    : BaseRepository<Cliente>(context), IClienteRepository
{
    public async Task<IEnumerable<Cliente>> GetAllWithVentasAsync(ClienteFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Clientes
            .Include(c => c.Ventas)
            .AsNoTracking()
            .AsQueryable();

        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                query = query.Where(c =>
                    c.RazonSocial.ToLower().Contains(term) ||
                    c.DocumentoIdentidad.ToLower().Contains(term) ||
                    c.Email.ToLower().Contains(term) ||
                    c.Telefono.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(filter.Documento))
            {
                var doc = filter.Documento.Trim().ToLower();
                query = query.Where(c => c.DocumentoIdentidad.ToLower().Contains(doc));
            }

            if (!string.IsNullOrWhiteSpace(filter.RazonSocial))
            {
                var name = filter.RazonSocial.Trim().ToLower();
                query = query.Where(c => c.RazonSocial.ToLower().Contains(name));
            }

            query = filter.OrderBy switch
            {
                "nombre_desc" => query.OrderByDescending(c => c.RazonSocial),
                "documento_asc" => query.OrderBy(c => c.DocumentoIdentidad),
                "documento_desc" => query.OrderByDescending(c => c.DocumentoIdentidad),
                "ventas_desc" => query.OrderByDescending(c => c.Ventas.Count),
                _ => query.OrderBy(c => c.RazonSocial)
            };
        }
        else
        {
            query = query.OrderBy(c => c.RazonSocial);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Cliente?> GetByIdWithVentasAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Clientes
            .Include(c => c.Ventas)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Cliente?> GetByDocumentoAsync(string documento, CancellationToken cancellationToken = default)
    {
        var doc = documento.Trim().ToLower();
        return await _context.Clientes
            .Include(c => c.Ventas)
            .FirstOrDefaultAsync(c => c.DocumentoIdentidad.ToLower() == doc, cancellationToken);
    }

    public async Task<bool> DocumentoExistsAsync(string documento, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var doc = documento.Trim().ToLower();
        var query = _context.Clientes.Where(c => c.DocumentoIdentidad.ToLower() == doc);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}
