using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ElTrueque.Api.Data;
using ElTrueque.Api.Models;
using ElTrueque.Api.Services;
using System.Threading.Tasks;
using System.Linq;

namespace Feriando.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ElTruequeDbContext _context;

        public ChatController(ElTruequeDbContext context)
        {
            _context = context;
        }

        // GET /api/chat/historial/{chatId}
        [HttpGet("historial/{chatId}")]
        public async Task<IActionResult> ObtenerHistorial(string chatId)
        {
            var mensajes = await _context.MensajesChat
                .Where(m => m.ChatId == chatId)
                .OrderBy(m => m.FechaEnvio)
                .Select(m => new
                {
                    m.Id,
                    m.ChatId,
                    m.EmisorId,
                    m.Mensaje,
                    FechaEnvio = m.FechaEnvio.ToString("yyyy-MM-dd HH:mm:ss"),
                    m.Leido
                })
                .ToListAsync();

            return Ok(mensajes);
        }

        [Authorize]
        [HttpGet("conversaciones")]
        public async Task<IActionResult> ListarConversaciones()
        {
            var usuarioId = User.GetUsuarioID();
            if (usuarioId <= 0) return Unauthorized();

            var marcadorParticipante = $"_usuarios_{usuarioId}_";
            var sufijoParticipante = $"_{usuarioId}";
            var mensajes = await _context.MensajesChat
                .Where(m => m.ChatId.StartsWith("producto_") &&
                    (m.ChatId.Contains(marcadorParticipante) || m.ChatId.EndsWith(sufijoParticipante)))
                .OrderByDescending(m => m.FechaEnvio)
                .ToListAsync();

            var conversaciones = new List<(string ChatId, int ProductoId, int ContactoId, MensajeChat UltimoMensaje)>();
            foreach (var ultimoMensaje in mensajes.GroupBy(m => m.ChatId).Select(g => g.First()))
            {
                var partesChat = ultimoMensaje.ChatId.Split("_usuarios_", 2);
                if (partesChat.Length != 2 || !partesChat[0].StartsWith("producto_") ||
                    !int.TryParse(partesChat[0]["producto_".Length..], out var productoId))
                {
                    continue;
                }

                var participantes = partesChat[1].Split('_');
                if (participantes.Length != 2 ||
                    !int.TryParse(participantes[0], out var primerParticipanteId) ||
                    !int.TryParse(participantes[1], out var segundoParticipanteId))
                {
                    continue;
                }

                var contactoId = primerParticipanteId == usuarioId
                    ? segundoParticipanteId
                    : segundoParticipanteId == usuarioId ? primerParticipanteId : 0;
                if (contactoId > 0)
                {
                    conversaciones.Add((ultimoMensaje.ChatId, productoId, contactoId, ultimoMensaje));
                }
            }

            var productoIds = conversaciones.Select(c => c.ProductoId).Distinct().ToList();
            var contactoIds = conversaciones.Select(c => c.ContactoId).Distinct().ToList();
            var productos = await _context.Productos
                .Where(p => productoIds.Contains(p.ProductoID))
                .Select(p => new { p.ProductoID, p.Nombre })
                .ToDictionaryAsync(p => p.ProductoID);
            var contactos = await _context.Usuarios
                .Where(u => contactoIds.Contains(u.UsuarioID))
                .Select(u => new { u.UsuarioID, u.Nombres, u.Apellidos })
                .ToDictionaryAsync(u => u.UsuarioID);

            var resultado = conversaciones
                .Where(c => productos.ContainsKey(c.ProductoId) && contactos.ContainsKey(c.ContactoId))
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
                    mensajesNoLeidos = mensajes.Count(m => m.ChatId == c.ChatId && m.EmisorId != usuarioId && !m.Leido)
                })
                .ToList();

            return Ok(resultado);
        }

        [Authorize]
        [HttpPost("conversaciones/{chatId}/leida")]
        public async Task<IActionResult> MarcarComoLeida(string chatId)
        {
            var usuarioId = User.GetUsuarioID();
            if (usuarioId <= 0) return Unauthorized();
            if (!EsParticipante(chatId, usuarioId)) return Forbid();

            var mensajes = await _context.MensajesChat
                .Where(m => m.ChatId == chatId && m.EmisorId != usuarioId && !m.Leido)
                .ToListAsync();

            foreach (var mensaje in mensajes)
            {
                mensaje.Leido = true;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static bool EsParticipante(string chatId, int usuarioId)
        {
            var partesChat = chatId.Split("_usuarios_", 2);
            if (partesChat.Length != 2 || !partesChat[0].StartsWith("producto_")) return false;

            var participantes = partesChat[1].Split('_');
            return participantes.Length == 2 &&
                int.TryParse(participantes[0], out var primero) &&
                int.TryParse(participantes[1], out var segundo) &&
                (primero == usuarioId || segundo == usuarioId);
        }
    }
}