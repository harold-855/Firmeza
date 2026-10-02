namespace Firmeza.Application.DTOS.Clientes;

public class ClienteDto
{
    public Guid Id { get; set; }
    public string DocumentoIdentidad { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string DireccionEnvio { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TotalCompras { get; set; }
    public decimal MontoTotalComprado { get; set; }
}
