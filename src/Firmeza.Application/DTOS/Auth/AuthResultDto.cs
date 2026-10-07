namespace Firmeza.Application.DTOS.Auth;

public class AuthResultDto
{
    public bool Succeeded { get; set; }
    public string? Message { get; set; }
    public IEnumerable<string> Errors { get; set; } = [];
    public string? Token { get; set; }
    public DateTime? TokenExpiration { get; set; }
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
    public IList<string> Roles { get; set; } = [];
    public bool IsClientBlockedFromAdmin { get; set; }

    public static AuthResultDto Success(
        string? userId = null,
        string? email = null,
        string? role = null,
        string? token = null,
        DateTime? expiration = null,
        IList<string>? roles = null) =>
        new()
        {
            Succeeded = true,
            UserId = userId,
            Email = email,
            Role = role,
            Token = token,
            TokenExpiration = expiration,
            Roles = roles ?? (role != null ? [role] : [])
        };

    public static AuthResultDto Failure(params string[] errors) =>
        new()
        {
            Succeeded = false,
            Errors = errors,
            Message = errors.Length > 0 ? errors[0] : "Ocurrió un error en la autenticación."
        };

    public static AuthResultDto ClientBlocked() =>
        new()
        {
            Succeeded = false,
            IsClientBlockedFromAdmin = true,
            Message = "Acceso denegado: Tu cuenta pertenece al perfil de Cliente. Los clientes deben utilizar el portal web / API para sus pedidos. El panel administrativo es exclusivo para Administradores.",
            Errors = ["Los usuarios con rol Cliente no tienen acceso al panel de administración Razor."]
        };
}
