namespace Firmeza.Application.DTOS.Productos;

public class ProductoDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string UnidadMedida { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int StockActual { get; set; }
    public bool Activo { get; set; }
    public int TotalVentasAsociadas { get; set; }
}
