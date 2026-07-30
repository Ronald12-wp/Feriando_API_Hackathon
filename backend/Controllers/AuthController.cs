using ElTrueque.Api.DTOs;
using ElTrueque.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElTrueque.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST api/auth/registro
    [HttpPost("registro")]
    public async Task<ActionResult<AuthResponse>> Registro(RegistroRequest request)
    {
        var resultado = await _authService.RegistrarAsync(request);
        if (resultado is null)
            return Conflict(new { mensaje = "Ya existe una cuenta registrada con este número de teléfono." });

        return Ok(resultado);
    }

    // POST api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var resultado = await _authService.LoginAsync(request);
        if (resultado is null)
            return Unauthorized(new { mensaje = "Teléfono o contraseña incorrectos." });

        return Ok(resultado);
    }
}
