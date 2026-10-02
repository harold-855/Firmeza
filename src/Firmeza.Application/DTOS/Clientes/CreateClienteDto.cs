using System.ComponentModel.DataAnnotations;

namespace Firmeza.Application.DTOS.Clientes;

public class CreateClienteDto
{
    [Required(ErrorMessage = "El documento de identidad o NIT es obligatorio.")]
    [StringLength(50, MinimumLength = 5, ErrorMessage = "El documento debe tener entre 5 y 50 caracteres.")]
    [RegularExpression(@"^[a-zA-Z0-9\-\.]+$", ErrorMessage = "El documento solo puede contener números, letras, guiones y puntos.")]
    [Display(Name = "Documento de Identidad / NIT")]
    public string DocumentoIdentidad { get; set; } = string.Empty;

    [Required(ErrorMessage = "La razón social o nombre completo es obligatorio.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "La razón social debe tener entre 3 y 200 caracteres.")]
    [Display(Name = "Razón Social / Nombre Completo")]
    public string RazonSocial { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono de contacto es obligatorio.")]
    [Phone(ErrorMessage = "El número telefónico no tiene un formato válido.")]
    [RegularExpression(@"^[0-9\+\-\s\(\)]{7,20}$", ErrorMessage = "El teléfono debe contener entre 7 y 20 dígitos numéricos válidos.")]
    [Display(Name = "Teléfono de Contacto")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección de envío o despacho es obligatoria.")]
    [StringLength(250, MinimumLength = 5, ErrorMessage = "La dirección de envío debe tener entre 5 y 250 caracteres.")]
    [Display(Name = "Dirección de Despacho / Obra")]
    public string DireccionEnvio { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo electrónico ingresado no tiene un formato válido (ej: cliente@empresa.com).")]
    [StringLength(150, ErrorMessage = "El correo no puede exceder los 150 caracteres.")]
    [Display(Name = "Correo Electrónico")]
    public string Email { get; set; } = string.Empty;
}
