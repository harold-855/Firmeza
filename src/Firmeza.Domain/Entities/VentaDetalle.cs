using Firmeza.Domain.Shared;

namespace Firmeza.Domain.Entities;

public class VentaDetalle : BaseEntity
{
    public int Cantidad { get; set; }
    public decimal PrecioAplicado { get; set; } // Precio histórico al momento de comprar
    public decimal Subtotal => Cantidad * PrecioAplicado;

    // Relación con Venta (Muchos a Uno)
    public Guid VentaId { get; set; }
    public Venta Venta { get; set; } = null!;

    // Relación con Producto (Muchos a Uno)
    public Guid ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
}