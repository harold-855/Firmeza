using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Firmeza.Application.DTOS.Auth;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using Microsoft.Extensions.Logging;

namespace Firmeza.Infrastructure.Identity;

public class AuthService(
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    RoleManager<IdentityRole> roleManager,
    IConfiguration configuration,
    IEmailService? emailService = null,
    ILogger<AuthService>? logger = null) : IAuthService
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
            // El usuario existe y su contraseña es correcta, pero no es Administrador (es Cliente)
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

    public async Task<AuthResultDto> LoginJwtAsync(LoginDto loginDto)
    {
        var user = await userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
        {
            return AuthResultDto.Failure("Credenciales incorrectas o usuario no registrado.");
        }

        var isPasswordValid = await userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!isPasswordValid)
        {
            return AuthResultDto.Failure("Credenciales incorrectas.");
        }

        var userRoles = await userManager.GetRolesAsync(user);
        var primaryRole = userRoles.FirstOrDefault() ?? Roles.Cliente;

        var (token, expiration) = GenerateJwtToken(user, userRoles);

        return AuthResultDto.Success(
            userId: user.Id,
            email: user.Email,
            role: primaryRole,
            token: token,
            expiration: expiration,
            roles: userRoles);
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

        var userRoles = await userManager.GetRolesAsync(user);
        var (token, expiration) = GenerateJwtToken(user, userRoles);

        if (emailService != null && !string.IsNullOrWhiteSpace(user.Email))
        {
            try
            {
                await emailService.SendWelcomeEmailAsync(user.Email, user.Email);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "No se pudo enviar el correo de bienvenida al registrar el usuario {Email}", user.Email);
            }
        }

        return AuthResultDto.Success(
            userId: user.Id,
            email: user.Email,
            role: role,
            token: token,
            expiration: expiration,
            roles: userRoles);
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

    private (string Token, DateTime Expiration) GenerateJwtToken(IdentityUser user, IList<string> roles)
    {
        var jwtKey = configuration["Jwt:Key"] ?? "Firmeza_Secret_Key_Super_Segura_2026_JWT_Token_Key_Materiales_Default_Secret_Key_123456789";
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "Firmeza.Api";
        var jwtAudience = configuration["Jwt:Audience"] ?? "Firmeza.ClientApp";
        var durationInDays = int.TryParse(configuration["Jwt:DurationInDays"], out var days) ? days : 7;

        var expiration = DateTime.UtcNow.AddDays(durationInDays);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            Issuer = jwtIssuer,
            Audience = jwtAudience,
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return (tokenHandler.WriteToken(token), expiration);
    }
}
