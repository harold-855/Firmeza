using Firmeza.Application.DTOS.Auth;

namespace Firmeza.Application.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// Inicia sesión para administradores en el panel Razor MVC mediante cookies.
    /// </summary>
    Task<AuthResultDto> LoginAdminAsync(LoginDto loginDto);

    /// <summary>
    /// Inicia sesión y emite un token JWT Bearer firmado (válido tanto para Administrador como para Cliente).
    /// </summary>
    Task<AuthResultDto> LoginJwtAsync(LoginDto loginDto);

    /// <summary>
    /// Registra un nuevo usuario con el rol especificado (por defecto 'Cliente' o 'Administrador').
    /// </summary>
    Task<AuthResultDto> RegisterAsync(RegisterDto registerDto, string role);

    /// <summary>
    /// Cierra la sesión activa (cookies de Identity).
    /// </summary>
    Task LogoutAsync();

    /// <summary>
    /// Verifica si un correo pertenece a un rol determinado.
    /// </summary>
    Task<bool> IsUserInRoleAsync(string email, string role);
}
