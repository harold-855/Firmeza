namespace Firmeza.Application.DTOS.Productos;

public class ProductoFilterDto
{
    public string? SearchTerm { get; set; }
    public string? UnidadMedida { get; set; }
    public bool? SoloActivos { get; set; }
    public bool? SoloBajoStock { get; set; }
    public int UmbralBajoStock { get; set; } = 50;
    public decimal? PrecioMin { get; set; }
    public decimal? PrecioMax { get; set; }
    public string? OrderBy { get; set; } // "nombre_asc", "nombre_desc", "precio_asc", "precio_desc", "stock_asc", "stock_desc"
}
