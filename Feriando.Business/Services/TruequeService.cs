using ElTrueque.Api.Data;
using ElTrueque.Api.DTOs;
using ElTrueque.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ElTrueque.Api.Services;

public interface ITruequeService
{
    Task<(bool exito, string mensaje, TruequeResponse? trueque)> SolicitarAsync(int usuarioSolicitanteID, TruequeCreateRequest request);
    Task<(bool exito, string mensaje, TruequeResponse? trueque)> ResponderAsync(int usuarioReceptorID, int truequeID, TruequeRespuestaRequest request);
    Task<List<TruequeResponse>> ListarPorUsuarioAsync(int usuarioID);
}

public class TruequeService : ITruequeService
{
    private readonly ElTruequeDbContext _db;
    private readonly INotificacionService _notificaciones;

    public TruequeService(ElTruequeDbContext db, INotificacionService notificaciones)
    {
        _db = db;
        _notificaciones = notificaciones;
    }

    public async Task<(bool, string, TruequeResponse?)> SolicitarAsync(int usuarioSolicitanteID, TruequeCreateRequest request)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var productoOfertado = await _db.Productos
            .Include(p => p.Usuario)
            .FirstOrDefaultAsync(p => p.ProductoID == request.ProductoOfertadoID);

        if (productoOfertado is null)
            return (false, "El producto ofertado no existe.", null);

        if (productoOfertado.Estado != "Disponible")
            return (false, "El producto ya no está disponible.", null);

        Producto? productoSolicitado = null;
        int usuarioReceptorID;

        if (request.ProductoSolicitadoID.HasValue)
        {
            if (productoOfertado.UsuarioID != usuarioSolicitanteID)
                return (false, "Solo puedes ofrecer un producto que te pertenece.", null);

            productoSolicitado = await _db.Productos
                .FirstOrDefaultAsync(p => p.ProductoID == request.ProductoSolicitadoID.Value);

            if (productoSolicitado is null)
                return (false, "El producto solicitado no existe.", null);

            if (productoSolicitado.Estado != "Disponible")
                return (false, "El producto solicitado ya no está disponible.", null);

            if (productoSolicitado.UsuarioID == usuarioSolicitanteID)
                return (false, "No puedes solicitar tu propio producto.", null);

            usuarioReceptorID = productoSolicitado.UsuarioID;
        }
        else
        {
            // Compra directa: ProductoOfertadoID identifica aquí el producto de la vendedora.
            if (productoOfertado.UsuarioID == usuarioSolicitanteID)
                return (false, "No puedes comprar tu propio producto.", null);

            if (productoOfertado.TipoOferta == "Trueque")
                return (false, "Este producto no está publicado para venta directa.", null);

            usuarioReceptorID = productoOfertado.UsuarioID;
        }

        var trueque = new Trueque
        {
            ProductoOfertadoID = productoOfertado.ProductoID,
            ProductoSolicitadoID = productoSolicitado?.ProductoID,
            UsuarioSolicitanteID = usuarioSolicitanteID,
            UsuarioReceptorID = usuarioReceptorID,
            Estado = "Pendiente",
            MontoAdicional = request.MontoAdicional ?? 0,
            LugarEncuentro = request.LugarEncuentro,
            FechaSolicitud = DateTime.UtcNow
        };

        _db.Trueques.Add(trueque);

        productoOfertado.Estado = "Reservado";
        if (productoSolicitado is not null) productoSolicitado.Estado = "Reservado";

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        await _notificaciones.NotificarAsync(
            usuarioReceptorID,
            "Nueva solicitud de intercambio",
            $"Tienes una nueva solicitud por tu producto \"{(productoSolicitado ?? productoOfertado).Nombre}\".",
            trueque.TruequeID);

        return (true, "Solicitud enviada.", await ObtenerRespuestaAsync(trueque.TruequeID));
    }

    public async Task<(bool, string, TruequeResponse?)> ResponderAsync(int usuarioReceptorID, int truequeID, TruequeRespuestaRequest request)
    {
        var trueque = await _db.Trueques
            .Include(t => t.ProductoOfertado)
            .Include(t => t.ProductoSolicitado)
            .FirstOrDefaultAsync(t => t.TruequeID == truequeID);

        if (trueque is null) return (false, "Trueque no encontrado.", null);

        if (trueque.UsuarioReceptorID != usuarioReceptorID)
            return (false, "No tienes permiso para responder esta solicitud.", null);

        if (trueque.Estado != "Pendiente")
            return (false, "Esta solicitud ya fue respondida anteriormente.", null);

        if (request.Accion.Equals("Aceptar", StringComparison.OrdinalIgnoreCase))
        {
            trueque.Estado = "Aceptado";
            trueque.LugarEncuentro = request.LugarEncuentro ?? trueque.LugarEncuentro;
            trueque.FechaRespuesta = DateTime.UtcNow;

            await _notificaciones.NotificarAsync(
                trueque.UsuarioSolicitanteID,
                "¡Tu solicitud fue aceptada!",
                $"Coordina la entrega de \"{trueque.ProductoOfertado!.Nombre}\".",
                trueque.TruequeID);
        }
        else if (request.Accion.Equals("Rechazar", StringComparison.OrdinalIgnoreCase))
        {
            trueque.Estado = "Rechazado";
            trueque.FechaRespuesta = DateTime.UtcNow;

            trueque.ProductoOfertado!.Estado = "Disponible";
            if (trueque.ProductoSolicitado is not null) trueque.ProductoSolicitado.Estado = "Disponible";

            await _notificaciones.NotificarAsync(
                trueque.UsuarioSolicitanteID,
                "Solicitud rechazada",
                $"Tu solicitud por \"{trueque.ProductoOfertado.Nombre}\" fue rechazada.",
                trueque.TruequeID);
        }
        else
        {
            return (false, "Acción inválida. Usa 'Aceptar' o 'Rechazar'.", null);
        }

        await _db.SaveChangesAsync();
        return (true, "Respuesta registrada.", await ObtenerRespuestaAsync(trueque.TruequeID));
    }

    public async Task<List<TruequeResponse>> ListarPorUsuarioAsync(int usuarioID)
    {
        var trueques = await _db.Trueques
            .Include(t => t.ProductoOfertado)
            .Include(t => t.ProductoSolicitado)
            .Include(t => t.UsuarioSolicitante)
            .Include(t => t.UsuarioReceptor)
            .Include(t => t.Valoraciones)
            .Where(t => t.UsuarioSolicitanteID == usuarioID || t.UsuarioReceptorID == usuarioID)
            .OrderByDescending(t => t.FechaSolicitud)
            .ToListAsync();

        return trueques.Select(t => MapearRespuesta(
            t,
            t.Valoraciones.Any(v => v.UsuarioEvaluadorID == usuarioID))).ToList();
    }

    private async Task<TruequeResponse?> ObtenerRespuestaAsync(int truequeID)
    {
        var trueque = await _db.Trueques
            .Include(t => t.ProductoOfertado)
            .Include(t => t.ProductoSolicitado)
            .Include(t => t.UsuarioSolicitante)
            .Include(t => t.UsuarioReceptor)
            .FirstOrDefaultAsync(t => t.TruequeID == truequeID);

        return trueque is null ? null : MapearRespuesta(trueque);
    }

    private static TruequeResponse MapearRespuesta(Trueque t, bool yaValore = false) => new()
    {
        TruequeID = t.TruequeID,
        Estado = t.Estado,
        YaValore = yaValore,
        ProductoOfertadoID = t.ProductoOfertadoID,
        ProductoOfertadoNombre = t.ProductoOfertado?.Nombre ?? string.Empty,
        ProductoSolicitadoID = t.ProductoSolicitadoID,
        ProductoSolicitadoNombre = t.ProductoSolicitado?.Nombre,
        UsuarioSolicitanteID = t.UsuarioSolicitanteID,
        UsuarioSolicitanteNombre = t.UsuarioSolicitante is null ? string.Empty : $"{t.UsuarioSolicitante.Nombres} {t.UsuarioSolicitante.Apellidos}",
        UsuarioReceptorID = t.UsuarioReceptorID,
        UsuarioReceptorNombre = t.UsuarioReceptor is null ? string.Empty : $"{t.UsuarioReceptor.Nombres} {t.UsuarioReceptor.Apellidos}",
        MontoAdicional = t.MontoAdicional,
        LugarEncuentro = t.LugarEncuentro,
        FechaSolicitud = t.FechaSolicitud,
        FechaRespuesta = t.FechaRespuesta,
        FechaCompletado = t.FechaCompletado
    };
}
