using ElTrueque.Api.Data;
using ElTrueque.Api.Models;

namespace ElTrueque.Api.Services;

public interface INotificacionService
{
    Task NotificarAsync(int usuarioID, string titulo, string mensaje, int? truequeID = null);
}

public class NotificacionService : INotificacionService
{
    private readonly ElTruequeDbContext _db;

    public NotificacionService(ElTruequeDbContext db)
    {
        _db = db;
    }

    public async Task NotificarAsync(int usuarioID, string titulo, string mensaje, int? truequeID = null)
    {
        _db.Notificaciones.Add(new Notificacion
        {
            UsuarioID = usuarioID,
            Titulo = titulo,
            Mensaje = mensaje,
            TruequeID = truequeID,
            Leido = false,
            FechaCreacion = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        // Punto de extensión: aquí se integraría Firebase Cloud Messaging
        // para enviar el push a la app móvil, además de guardar el registro.
    }
}
