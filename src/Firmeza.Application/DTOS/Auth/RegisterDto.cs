using System.ComponentModel.DataAnnotations;

namespace Firmeza.Application.DTOS.Auth;

public class RegisterDto
{
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La edad es obligatoria.")]
    [Display(Name = "Edad")]
    public string Edad { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y máximo {1} caracteres.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirmar contraseña")]
    [Compare("Password", ErrorMessage = "La contraseña y la confirmación de contraseña no coinciden.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? Role { get; set; }
}
