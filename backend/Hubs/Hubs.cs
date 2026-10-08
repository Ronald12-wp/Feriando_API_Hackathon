using ElTrueque.Api.Data;
using ElTrueque.Api.Models;
using ElTrueque.Api.Services;
using Feriando.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Feriando.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private const int LongitudMaximaMensaje = 4000;
    private readonly ElTruequeDbContext _context;

    public ChatHub(ElTruequeDbContext context)
    {
        _context = context;
    }

    public async Task UnirseAChat(string chatId)
    {
        var usuarioID = Context.User!.GetUsuarioID();
        if (!await ChatAccess.PuedeAccederAsync(_context, chatId, usuarioID, Context.ConnectionAborted))
            throw new HubException("No tienes permiso para acceder a esta conversación.");

        await Groups.AddToGroupAsync(Context.ConnectionId, chatId, Context.ConnectionAborted);
    }

    public async Task SalirDeChat(string chatId)
    {
        var usuarioID = Context.User!.GetUsuarioID();
        if (!await ChatAccess.PuedeAccederAsync(_context, chatId, usuarioID, Context.ConnectionAborted))
            throw new HubException("No tienes permiso para acceder a esta conversación.");

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId, Context.ConnectionAborted);
    }

    public async Task EnviarMensaje(string chatId, string mensaje)
    {
        var usuarioID = Context.User!.GetUsuarioID();
        if (!await ChatAccess.PuedeAccederAsync(_context, chatId, usuarioID, Context.ConnectionAborted))
            throw new HubException("No tienes permiso para enviar mensajes en esta conversación.");

        var texto = mensaje?.Trim();
        if (string.IsNullOrWhiteSpace(texto) || texto.Length > LongitudMaximaMensaje)
            throw new HubException($"El mensaje debe tener entre 1 y {LongitudMaximaMensaje} caracteres.");

        var fecha = DateTime.UtcNow;
        _context.MensajesChat.Add(new MensajeChat
        {
            ChatId = chatId,
            EmisorId = usuarioID,
            Mensaje = texto,
            FechaEnvio = fecha,
            Leido = false
        });

        await _context.SaveChangesAsync(Context.ConnectionAborted);
        await Clients.Group(chatId).SendAsync(
            "RecibirMensaje",
            usuarioID,
            texto,
            fecha.ToString("yyyy-MM-dd HH:mm:ss"),
            Context.ConnectionAborted);
    }
}
