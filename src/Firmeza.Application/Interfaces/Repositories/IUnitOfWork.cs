namespace Firmeza.Application.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IClienteRepository Clientes { get; }
    IProductoRepository Productos { get; }
    IVentaRepository Ventas { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
