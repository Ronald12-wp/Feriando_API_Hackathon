using ElTrueque.Api.Data;
using ElTrueque.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElTrueque.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificacionesController : ControllerBase
{
    private readonly ElTruequeDbContext _db;

    public NotificacionesController(ElTruequeDbContext db)
    {
        _db = db;
    }

    // GET api/notificaciones  -> las notificaciones de la usuaria autenticada
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] bool soloNoLeidas = false)
    {
        int usuarioID = User.GetUsuarioID();

        var query = _db.Notificaciones.Where(n => n.UsuarioID == usuarioID);
        if (soloNoLeidas) query = query.Where(n => !n.Leido);

        var notificaciones = await query
            .OrderByDescending(n => n.FechaCreacion)
            .ToListAsync();

        return Ok(notificaciones);
    }

    // PUT api/notificaciones/5/leida
    [HttpPut("{id:int}/leida")]
    public async Task<IActionResult> MarcarLeida(int id)
    {
        int usuarioID = User.GetUsuarioID();
        var notificacion = await _db.Notificaciones
            .FirstOrDefaultAsync(n => n.NotificacionID == id && n.UsuarioID == usuarioID);

        if (notificacion is null) return NotFound();

        notificacion.Leido = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
