using ElTrueque.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Feriando.Api.Services;

public sealed record ParticipantesChat(int ProductoID, int PrimerUsuarioID, int SegundoUsuarioID)
{
    public int OtroUsuario(int usuarioID) =>
        usuarioID == PrimerUsuarioID ? SegundoUsuarioID : PrimerUsuarioID;
}

public static class ChatAccess
{
    public static bool TryParse(string chatId, out ParticipantesChat participantes)
    {
        participantes = new ParticipantesChat(0, 0, 0);
        const string prefijo = "producto_";
        const string separador = "_usuarios_";

        if (string.IsNullOrWhiteSpace(chatId) || !chatId.StartsWith(prefijo, StringComparison.Ordinal))
            return false;

        var indiceSeparador = chatId.IndexOf(separador, StringComparison.Ordinal);
        if (indiceSeparador <= prefijo.Length ||
            chatId.IndexOf(separador, indiceSeparador + separador.Length, StringComparison.Ordinal) >= 0)
            return false;

        var productoTexto = chatId[prefijo.Length..indiceSeparador];
        var idsTexto = chatId[(indiceSeparador + separador.Length)..].Split('_');
        if (idsTexto.Length != 2 ||
            !int.TryParse(productoTexto, out var productoID) || productoID <= 0 ||
            !int.TryParse(idsTexto[0], out var primero) || primero <= 0 ||
            !int.TryParse(idsTexto[1], out var segundo) || segundo <= 0 || primero >= segundo)
            return false;

        var esperado = $"producto_{productoID}_usuarios_{primero}_{segundo}";
        if (!string.Equals(chatId, esperado, StringComparison.Ordinal))
            return false;

        participantes = new ParticipantesChat(productoID, primero, segundo);
        return true;
    }

    public static async Task<bool> PuedeAccederAsync(
        ElTruequeDbContext db,
        string chatId,
        int usuarioID,
        CancellationToken cancellationToken = default)
    {
        if (!TryParse(chatId, out var participantes) ||
            (usuarioID != participantes.PrimerUsuarioID && usuarioID != participantes.SegundoUsuarioID))
            return false;

        var ids = new[] { participantes.PrimerUsuarioID, participantes.SegundoUsuarioID };
        var usuariosActivos = await db.Usuarios
            .CountAsync(u => ids.Contains(u.UsuarioID) && u.EstadoActivo, cancellationToken);
        if (usuariosActivos != 2)
            return false;

        return await db.Productos.AnyAsync(
            p => p.ProductoID == participantes.ProductoID && ids.Contains(p.UsuarioID),
            cancellationToken);
    }
}
