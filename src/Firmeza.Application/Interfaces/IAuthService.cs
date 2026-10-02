using Firmeza.Application.DTOS.Auth;

namespace Firmeza.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResultDto> LoginAdminAsync(LoginDto loginDto);
    Task<AuthResultDto> RegisterAsync(RegisterDto registerDto, string role);
    Task LogoutAsync();
    Task<bool> IsUserInRoleAsync(string email, string role);
}
