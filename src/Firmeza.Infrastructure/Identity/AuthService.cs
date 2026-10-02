using Firmeza.Application.DTOS.Auth;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Constants;
using Microsoft.AspNetCore.Identity;

namespace Firmeza.Infrastructure.Identity;

public class AuthService(
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    RoleManager<IdentityRole> roleManager) : IAuthService
{
    public async Task<AuthResultDto> LoginAdminAsync(LoginDto loginDto)
    {
        var user = await userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
        {
            return AuthResultDto.Failure("Credenciales incorrectas o usuario no encontrado.");
        }

        var isPasswordValid = await userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!isPasswordValid)
        {
            return AuthResultDto.Failure("Credenciales incorrectas.");
        }

        var isAdmin = await userManager.IsInRoleAsync(user, Roles.Administrador);
        if (!isAdmin)
        {
            // El usuario existe y su contraseña es correcta, pero no es Administrador (es Cliente u otro rol)
            // Se bloquea el acceso al panel Razor
            return AuthResultDto.ClientBlocked();
        }

        // Si es administrador, realizamos el inicio de sesión por cookie en Razor
        var signInResult = await signInManager.PasswordSignInAsync(
            user.UserName ?? user.Email!,
            loginDto.Password,
            loginDto.RememberMe,
            lockoutOnFailure: false);

        if (!signInResult.Succeeded)
        {
            return AuthResultDto.Failure("No se pudo iniciar sesión. Verifique su cuenta o estado.");
        }

        return AuthResultDto.Success(user.Id, user.Email, Roles.Administrador);
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterDto registerDto, string role)
    {
        var existingUser = await userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            return AuthResultDto.Failure("El correo electrónico ya está registrado.");
        }

        var user = new IdentityUser
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, registerDto.Password);
        if (!createResult.Succeeded)
        {
            return AuthResultDto.Failure(createResult.Errors.Select(e => e.Description).ToArray());
        }

        // Asegurar que el rol exista
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }

        await userManager.AddToRoleAsync(user, role);

        return AuthResultDto.Success(user.Id, user.Email, role);
    }

    public async Task LogoutAsync()
    {
        await signInManager.SignOutAsync();
    }

    public async Task<bool> IsUserInRoleAsync(string email, string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return false;
        }

        return await userManager.IsInRoleAsync(user, role);
    }
}
