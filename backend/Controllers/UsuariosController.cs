using ElTrueque.Api.DTOs;
using ElTrueque.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElTrueque.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(IUsuarioService usuarioService, ILogger<UsuariosController> logger)
    {
        _usuarioService = usuarioService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el perfil del usuario autenticado
    /// </summary>
    [HttpGet("perfil")]
    public async Task<ActionResult<UsuarioResponse>> ObtenerPerfil()
    {
        try
        {
            int usuarioID = HttpContext.User.GetUsuarioID();
            var perfil = await _usuarioService.ObtenerPerfilAsync(usuarioID);

            if (perfil is null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            return Ok(perfil);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener perfil");
            return StatusCode(500, new { mensaje = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// Actualiza la foto de perfil del usuario autenticado
    /// </summary>
    [HttpPut("fotoPerfil")]
    public async Task<ActionResult<UsuarioResponse>> ActualizarFotoPerfil(IFormFile fotoPerfil)
    {
        try
        {
            if (fotoPerfil is null || fotoPerfil.Length == 0)
                return BadRequest(new { mensaje = "No se proporcionó imagen" });

            int usuarioID = HttpContext.User.GetUsuarioID();
            var resultado = await _usuarioService.ActualizarFotoPerfilAsync(usuarioID, fotoPerfil);

            if (resultado is null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            return Ok(resultado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar foto de perfil");
            return StatusCode(500, new { mensaje = "Error al actualizar la foto" });
        }
    }

    /// <summary>
    /// Actualiza los datos del perfil del usuario autenticado
    /// </summary>
    [HttpPut("perfil")]
    [Consumes("application/json")]
    public async Task<ActionResult<UsuarioResponse>> ActualizarPerfil([FromBody] ActualizarPerfilRequest request)
    {
        try
        {
            if (request is null)
                return BadRequest(new { mensaje = "Datos inválidos" });

            int usuarioID = HttpContext.User.GetUsuarioID();
            var resultado = await _usuarioService.ActualizarPerfilAsync(usuarioID, request);

            if (resultado is null)
                return NotFound(new { mensaje = "Usuario no encontrado" });

            return Ok(resultado);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar perfil");
            return StatusCode(500, new { mensaje = "Error al actualizar el perfil" });
        }
    }
}
