using Firmeza.Domain.Shared;

namespace Firmeza.Domain.Entities;

public class Producto: BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string UnidadMedida { get; set; } = string.Empty; 
    public decimal PrecioUnitario { get; set; }
    public int StockActual { get; set; }
    public bool Activo { get; set; } = true;

    // Relación: Un producto puede estar en muchos detalles de venta
    public ICollection<VentaDetalle> DetallesVenta { get; set; } = new List<VentaDetalle>();
}