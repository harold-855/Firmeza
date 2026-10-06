namespace Firmeza.Application.DTOS.Importacion;

public class ExcelImportOptionsDto
{
    /// <summary>
    /// Si es verdadero, actualiza los datos de productos y clientes si ya existen en la base de datos.
    /// Si es falso, no modifica los registros existentes y solo inserta los nuevos.
    /// </summary>
    public bool ActualizarSiExiste { get; set; } = true;

    /// <summary>
    /// Si es verdadero, procesa y crea las ventas cuando se detecten cantidades y relaciones cliente-producto.
    /// </summary>
    public bool CrearVentasSiHayDatos { get; set; } = true;

    /// <summary>
    /// Si es verdadero, descuenta del stock del producto las unidades vendidas en las nuevas ventas registradas.
    /// </summary>
    public bool DescontarStockDeVentas { get; set; } = true;

    /// <summary>
    /// Nombre opcional de una hoja específica a importar. Si es null o vacío, procesa todas las hojas del libro.
    /// </summary>
    public string? NombreHoja { get; set; }
}
