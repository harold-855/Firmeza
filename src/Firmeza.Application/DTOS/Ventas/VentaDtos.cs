namespace Firmeza.Application.DTOS.Ventas;

public class VentaDetalleDto
{
    public Guid Id { get; set; }
    public Guid ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string UnidadMedida { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioAplicado { get; set; }
    public decimal Subtotal => Cantidad * PrecioAplicado;
}

public class VentaDto
{
    public Guid Id { get; set; }
    public string NumeroComprobante => $"REC-{Id.ToString()[..8].ToUpperInvariant()}";
    public DateTime FechaVenta { get; set; }
    public decimal Total { get; set; }
    public decimal SubtotalBase => Math.Round(Total / 1.19m, 2);
    public decimal Iva => Total - SubtotalBase;
    public string EstadoDespacho { get; set; } = "Pendiente";

    // Cliente
    public Guid ClienteId { get; set; }
    public string ClienteRazonSocial { get; set; } = string.Empty;
    public string ClienteDocumento { get; set; } = string.Empty;
    public string ClienteTelefono { get; set; } = string.Empty;
    public string ClienteDireccion { get; set; } = string.Empty;
    public string ClienteEmail { get; set; } = string.Empty;

    // Detalles
    public List<VentaDetalleDto> Detalles { get; set; } = new();
    public int TotalItems => Detalles.Sum(d => d.Cantidad);
    public string? RutaArchivoRecibo { get; set; }
}

public class CreateVentaDetalleDto
{
    public Guid ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal? PrecioAplicado { get; set; }
}

public class CreateVentaDto
{
    public Guid ClienteId { get; set; }
    public string EstadoDespacho { get; set; } = "Pendiente";
    public List<CreateVentaDetalleDto> Detalles { get; set; } = new();
}

public class VentaFilterDto
{
    public string? SearchTerm { get; set; }
    public Guid? ClienteId { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? EstadoDespacho { get; set; }
    public string? OrderBy { get; set; } = "fecha_desc";
}
