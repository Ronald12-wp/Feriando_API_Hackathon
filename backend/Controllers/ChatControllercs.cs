using ElTrueque.Api.Data;
using ElTrueque.Api.Models;
using ElTrueque.Api.Services;
using Feriando.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Feriando.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ElTruequeDbContext _context;

    public ChatController(ElTruequeDbContext context)
    {
        _context = context;
    }

    [HttpGet("historial/{chatId}")]
    public async Task<IActionResult> ObtenerHistorial(string chatId, CancellationToken cancellationToken)
    {
        var usuarioID = User.GetUsuarioID();
        if (!await ChatAccess.PuedeAccederAsync(_context, chatId, usuarioID, cancellationToken))
            return Forbid();

        var mensajes = await _context.MensajesChat
            .Where(m => m.ChatId == chatId)
            .OrderBy(m => m.FechaEnvio)
            .Select(m => new
            {
                m.Id,
                m.ChatId,
                m.EmisorId,
                m.Mensaje,
                m.FechaEnvio,
                m.Leido
            })
            .ToListAsync(cancellationToken);

        return Ok(mensajes);
    }

    [HttpGet("conversaciones")]
    public async Task<IActionResult> ListarConversaciones(CancellationToken cancellationToken)
    {
        var usuarioID = User.GetUsuarioID();
        if (usuarioID <= 0) return Unauthorized();

        var conversacionesOcultas = await _context.ConversacionesOcultas
            .Where(c => c.UsuarioID == usuarioID)
            .Select(c => c.ChatId)
            .ToListAsync(cancellationToken);

        var marcadorParticipante = $"_usuarios_{usuarioID}_";
        var sufijoParticipante = $"_{usuarioID}";
        var mensajes = await _context.MensajesChat
            .Where(m => m.ChatId.StartsWith("producto_") &&
                (m.ChatId.Contains(marcadorParticipante) || m.ChatId.EndsWith(sufijoParticipante)) &&
                !conversacionesOcultas.Contains(m.ChatId))
            .OrderByDescending(m => m.FechaEnvio)
            .ToListAsync(cancellationToken);

        var conversaciones = new List<(string ChatId, int ProductoId, int ContactoId, MensajeChat UltimoMensaje)>();
        foreach (var ultimoMensaje in mensajes.GroupBy(m => m.ChatId).Select(g => g.First()))
        {
            if (!ChatAccess.TryParse(ultimoMensaje.ChatId, out var participantes) ||
                (participantes.PrimerUsuarioID != usuarioID && participantes.SegundoUsuarioID != usuarioID))
                continue;

            conversaciones.Add((
                ultimoMensaje.ChatId,
                participantes.ProductoID,
                participantes.OtroUsuario(usuarioID),
                ultimoMensaje));
        }

        var productoIds = conversaciones.Select(c => c.ProductoId).Distinct().ToList();
        var contactoIds = conversaciones.Select(c => c.ContactoId).Distinct().ToList();
        var productos = await _context.Productos
            .Where(p => productoIds.Contains(p.ProductoID))
            .Select(p => new { p.ProductoID, p.Nombre, p.UsuarioID })
            .ToDictionaryAsync(p => p.ProductoID, cancellationToken);
        var contactos = await _context.Usuarios
            .Where(u => contactoIds.Contains(u.UsuarioID) && u.EstadoActivo)
            .Select(u => new { u.UsuarioID, u.Nombres, u.Apellidos })
            .ToDictionaryAsync(u => u.UsuarioID, cancellationToken);

        var resultado = conversaciones
            .Where(c => productos.ContainsKey(c.ProductoId) &&
                contactos.ContainsKey(c.ContactoId) &&
                (productos[c.ProductoId].UsuarioID == usuarioID ||
                 productos[c.ProductoId].UsuarioID == c.ContactoId))
            .Select(c => new
            {
                chatId = c.ChatId,
                productoId = c.ProductoId,
                nombreProducto = productos[c.ProductoId].Nombre,
                usuarioContactoId = c.ContactoId,
                nombreContacto = $"{contactos[c.ContactoId].Nombres} {contactos[c.ContactoId].Apellidos}".Trim(),
                ultimoMensaje = c.UltimoMensaje.Mensaje,
                fechaEnvio = c.UltimoMensaje.FechaEnvio,
                emisorId = c.UltimoMensaje.EmisorId,
                // El indicador representa una conversación con mensajes pendientes, no el total de mensajes.
                mensajesNoLeidos = mensajes.Any(m =>
                    m.ChatId == c.ChatId && m.EmisorId != usuarioID && !m.Leido) ? 1 : 0
            })
            .ToList();

        return Ok(resultado);
    }

    [HttpPost("conversaciones/{chatId}/leida")]
    public async Task<IActionResult> MarcarComoLeida(string chatId, CancellationToken cancellationToken)
    {
        var usuarioID = User.GetUsuarioID();
        if (!await ChatAccess.PuedeAccederAsync(_context, chatId, usuarioID, cancellationToken))
            return Forbid();

        var mensajes = await _context.MensajesChat
            .Where(m => m.ChatId == chatId && m.EmisorId != usuarioID && !m.Leido)
            .ToListAsync(cancellationToken);

        foreach (var mensaje in mensajes)
            mensaje.Leido = true;

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("conversaciones/{chatId}")]
    public async Task<IActionResult> OcultarConversacion(string chatId, CancellationToken cancellationToken)
    {
        var usuarioID = User.GetUsuarioID();
        if (!await ChatAccess.PuedeAccederAsync(_context, chatId, usuarioID, cancellationToken))
            return Forbid();

        var yaOculta = await _context.ConversacionesOcultas
            .AnyAsync(c => c.UsuarioID == usuarioID && c.ChatId == chatId, cancellationToken);
        if (!yaOculta)
        {
            _context.ConversacionesOcultas.Add(new ElTrueque.Api.Models.ConversacionOculta
            {
                UsuarioID = usuarioID,
                ChatId = chatId,
                FechaOcultacion = DateTime.UtcNow
            });
            await _context.SaveChangesAsync(cancellationToken);
        }

        return NoContent();
    }

    [HttpPost("conversaciones/{chatId}/restaurar")]
    public async Task<IActionResult> RestaurarConversacion(string chatId, CancellationToken cancellationToken)
    {
        var usuarioID = User.GetUsuarioID();
        if (!await ChatAccess.PuedeAccederAsync(_context, chatId, usuarioID, cancellationToken))
            return Forbid();

        var oculta = await _context.ConversacionesOcultas
            .FirstOrDefaultAsync(c => c.UsuarioID == usuarioID && c.ChatId == chatId, cancellationToken);
        if (oculta is not null)
        {
            _context.ConversacionesOcultas.Remove(oculta);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return NoContent();
    }
}
