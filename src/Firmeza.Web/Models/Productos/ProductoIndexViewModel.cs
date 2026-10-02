using Firmeza.Application.DTOS.Productos;

namespace Firmeza.Web.Models.Productos;

public class ProductoIndexViewModel
{
    public IEnumerable<ProductoDto> Productos { get; set; } = new List<ProductoDto>();
    public ProductoFilterDto Filter { get; set; } = new();
    public IEnumerable<string> UnidadesMedida { get; set; } = new List<string>();
    public int TotalProductos { get; set; }
    public int TotalActivos { get; set; }
    public int TotalBajoStock { get; set; }
}
