namespace Firmeza.Application.DTOS.Dashboard;

public class DashboardMetricsDto
{
    public int TotalProductos { get; set; }
    public int TotalClientes { get; set; }
    public int TotalVentas { get; set; }
    public decimal MontoTotalVentas { get; set; }
    public int DespachosPendientes { get; set; }
    public int DespachosEnRuta { get; set; }
    public int DespachosEntregados { get; set; }
    public List<VentaResumenDto> VentasRecientes { get; set; } = [];
    public List<ProductoResumenDto> ProductosBajoStock { get; set; } = [];
}

public class VentaResumenDto
{
    public Guid Id { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public decimal Total { get; set; }
    public string EstadoDespacho { get; set; } = string.Empty;
}

public class ProductoResumenDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int StockActual { get; set; }
    public decimal PrecioUnitario { get; set; }
    public string UnidadMedida { get; set; } = string.Empty;
}
