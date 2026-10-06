using Firmeza.Application.DTOS.Importacion;

namespace Firmeza.Application.Interfaces;

public interface IExcelImportService
{
    /// <summary>
    /// Lee un archivo Excel (.xlsx) con datos desorganizados o mezclados,
    /// normaliza automáticamente la información en memoria, valida los campos obligatorios,
    /// inserta o actualiza clientes, productos y ventas en base de datos, y genera un log de errores e inconsistencias.
    /// </summary>
    Task<ExcelImportResultDto> ImportarExcelDesorganizadoAsync(
        Stream excelStream,
        ExcelImportOptionsDto? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Genera un archivo Excel (.xlsx) de ejemplo con datos no normalizados (columnas mezcladas en una o varias hojas)
    /// para pruebas de importación y plantilla para los usuarios.
    /// </summary>
    byte[] GenerarPlantillaEjemplo();
}
