using Firmeza.Application.Interfaces.Repositories;
using Firmeza.Infrastructure.Persistence;

namespace Firmeza.Infrastructure.Repositories;

public class UnitOfWork(
    ApplicationDbContext context,
    IClienteRepository clienteRepository,
    IProductoRepository productoRepository,
    IVentaRepository ventaRepository) : IUnitOfWork
{
    private readonly ApplicationDbContext _context = context;
    private bool _disposed;

    public IClienteRepository Clientes { get; } = clienteRepository;
    public IProductoRepository Productos { get; } = productoRepository;
    public IVentaRepository Ventas { get; } = ventaRepository;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _context.Dispose();
        }
        _disposed = true;
    }
}
