using Firmeza.Application.DTOS.Clientes;

namespace Firmeza.Web.Models.Clientes;

public class ClienteIndexViewModel
{
    public IEnumerable<ClienteDto> Clientes { get; set; } = new List<ClienteDto>();
    public ClienteFilterDto Filter { get; set; } = new();
    public int TotalClientes { get; set; }
    public int ClientesConCompras { get; set; }
    public decimal MontoTotalAcumulado { get; set; }
}
