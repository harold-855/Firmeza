using System.ComponentModel.DataAnnotations;

namespace Firmeza.Application.DTOS.Productos;

public class CreateProductoDto
{
    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 150 caracteres.")]
    [Display(Name = "Nombre del Material / Producto")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
    [Display(Name = "Descripción Técnica")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La unidad de medida es obligatoria.")]
    [StringLength(50, ErrorMessage = "La unidad de medida no puede exceder 50 caracteres.")]
    [Display(Name = "Unidad de Medida (Bolsa, Millar, M3, Unidad, etc.)")]
    public string UnidadMedida { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio unitario es obligatorio.")]
    [Range(0.01, 100000000.0, ErrorMessage = "El precio unitario debe ser mayor a 0.")]
    [DataType(DataType.Currency)]
    [Display(Name = "Precio Unitario (COP)")]
    public decimal PrecioUnitario { get; set; }

    [Required(ErrorMessage = "El stock inicial es obligatorio.")]
    [Range(0, 1000000, ErrorMessage = "El stock no puede ser negativo.")]
    [Display(Name = "Stock Inicial")]
    public int StockActual { get; set; }

    [Display(Name = "¿Producto Activo para Venta?")]
    public bool Activo { get; set; } = true;
}
