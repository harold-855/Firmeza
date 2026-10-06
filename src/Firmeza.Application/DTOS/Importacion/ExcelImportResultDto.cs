namespace Firmeza.Application.DTOS.Importacion;

public class ExcelImportResultDto
{
    public bool Exitoso => Errores.Count(e => e.Nivel == NivelInconsistencia.Error) == 0;
    public int TotalHojasProcesadas { get; set; }
    public int TotalFilasLeidas { get; set; }
    public int TotalFilasProcesadasCorrectamente { get; set; }

    // Métricas de Clientes
    public int ClientesCreados { get; set; }
    public int ClientesActualizados { get; set; }
    public int ClientesOmitidos { get; set; }

    // Métricas de Productos
    public int ProductosCreados { get; set; }
    public int ProductosActualizados { get; set; }
    public int ProductosOmitidos { get; set; }

    // Métricas de Ventas
    public int VentasCreadas { get; set; }
    public int DetallesVentaCreados { get; set; }
    public decimal MontoTotalVentasImportadas { get; set; }

    // Log de inconsistencias y errores
    public List<ExcelImportErrorDto> Errores { get; set; } = new();

    public int TotalErrores => Errores.Count(e => e.Nivel == NivelInconsistencia.Error);
    public int TotalAdvertencias => Errores.Count(e => e.Nivel == NivelInconsistencia.Advertencia);

    public string Resumen { get; set; } = string.Empty;
    public DateTime FechaProcesamiento { get; set; } = DateTime.UtcNow;
}
