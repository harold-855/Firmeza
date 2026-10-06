namespace Firmeza.Application.DTOS.Importacion;

public enum NivelInconsistencia
{
    Error,
    Advertencia,
    Info
}

public class ExcelImportErrorDto
{
    public string Hoja { get; set; } = string.Empty;
    public int Fila { get; set; }
    public string Columna { get; set; } = string.Empty;
    public string Campo { get; set; } = string.Empty;
    public string? ValorInvalido { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public NivelInconsistencia Nivel { get; set; } = NivelInconsistencia.Error;
    public string EntidadAfectada { get; set; } = string.Empty; // "Cliente", "Producto", "Venta", "Estructura"
}
