using Firmeza.Domain.Shared;

namespace Firmeza.Domain.Entities;

public class Cliente : BaseEntity
{
    public string DocumentoIdentidad { get; set; } = string.Empty; // NIT, Cédula
    public string RazonSocial { get; set; } = string.Empty; // Nombre empresa o persona
    public string Telefono { get; set; } = string.Empty;
    public string DireccionEnvio { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Relación: Un cliente puede realizar muchas ventas
    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
}