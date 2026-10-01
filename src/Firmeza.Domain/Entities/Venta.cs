using Firmeza.Domain.Shared;

namespace Firmeza.Domain.Entities;

public class Venta : BaseEntity
{
    public DateTime FechaVenta { get; set; } = DateTime.UtcNow;
    public decimal Total { get; set; }
    public string EstadoDespacho { get; set; } = "Pendiente"; // Pendiente, En Ruta, Entregado
    
    // Relación con Cliente (Muchos a Uno)
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    // Relación: Una venta tiene muchos detalles (Uno a Muchos)
    public ICollection<VentaDetalle> Detalles { get; set; } = new List<VentaDetalle>();
}