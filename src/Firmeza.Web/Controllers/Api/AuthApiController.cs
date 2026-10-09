using System.Security.Claims;
using Firmeza.Application.DTOS.Auth;
using Firmeza.Application.Interfaces;
using Firmeza.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Web.Controllers.Api;

[ApiController]
[Route("api/auth")]
public class AuthApiController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await authService.LoginJwtAsync(model);
        if (!result.Succeeded)
        {
            return Unauthorized(new
            {
                mensaje = result.Message ?? "Credenciales inválidas.",
                errores = result.Errors
            });
        }

        return Ok(new
        {
            mensaje = "Autenticación exitosa.",
            token = result.Token,
            tokenExpiration = result.TokenExpiration,
            userId = result.UserId,
            email = result.Email,
            role = result.Role,
            roles = result.Roles
        });
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var rol = string.IsNullOrWhiteSpace(model.Role) ? Roles.Cliente : model.Role;
        var result = await authService.RegisterAsync(model, rol);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                mensaje = "No se pudo registrar el usuario.",
                errores = result.Errors
            });
        }

        return Ok(new
        {
            mensaje = $"Usuario registrado exitosamente con rol '{rol}'.",
            token = result.Token,
            tokenExpiration = result.TokenExpiration,
            userId = result.UserId,
            email = result.Email,
            role = result.Role,
            roles = result.Roles
        });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        return Ok(new
        {
            userId,
            email,
            roles,
            esAdministrador = roles.Contains(Roles.Administrador),
            esCliente = roles.Contains(Roles.Cliente)
        });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await authService.LogoutAsync();
        return Ok(new { mensaje = "Sesión finalizada correctamente." });
    }
}
