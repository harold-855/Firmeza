namespace Firmeza.Application.DTOS.Clientes;

public class ClienteFilterDto
{
    public string? SearchTerm { get; set; } // Busca por Nombre / Razón Social o Documento
    public string? Documento { get; set; }
    public string? RazonSocial { get; set; }
    public string? OrderBy { get; set; } // "nombre_asc", "nombre_desc", "documento_asc", "documento_desc", "ventas_desc"
}
