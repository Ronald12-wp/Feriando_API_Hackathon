using Microsoft.AspNetCore.SignalR;
using ElTrueque.Api.Data;
using ElTrueque.Api.Models;
using System;
using System.Threading.Tasks;

namespace Feriando.Api.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ElTruequeDbContext _context;

        public ChatHub(ElTruequeDbContext context)
        {
            _context = context;
        }

        public async Task UnirseAChat(string chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
        }

        public async Task SalirDeChat(string chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
        }

        public async Task EnviarMensaje(string chatId, int emisorId, string mensaje)
        {
            var fecha = DateTime.Now;

            // 1. Guardar mensaje en SQL Server
            var nuevoMensaje = new MensajeChat
            {
                ChatId = chatId,
                EmisorId = emisorId,
                Mensaje = mensaje,
                FechaEnvio = fecha,
                Leido = false
            };

            _context.MensajesChat.Add(nuevoMensaje);
            await _context.SaveChangesAsync();

            // 2. Retransmitir mensaje a los usuarios conectados a la sala
            string fechaFormateada = fecha.ToString("yyyy-MM-dd HH:mm:ss");
            await Clients.Group(chatId).SendAsync("RecibirMensaje", emisorId, mensaje, fechaFormateada);
        }
    }
}