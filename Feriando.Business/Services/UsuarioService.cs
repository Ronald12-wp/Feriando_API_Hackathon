using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using ElTrueque.Api.Data;
using ElTrueque.Api.DTOs;
using ElTrueque.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ElTrueque.Api.Services;

public interface IUsuarioService
{
    Task<UsuarioResponse?> ActualizarFotoPerfilAsync(int usuarioID, IFormFile imagen);
    Task<UsuarioResponse?> ActualizarPerfilAsync(int usuarioID, ActualizarPerfilRequest request);
    Task<UsuarioResponse?> ObtenerPerfilAsync(int usuarioID);
}

public class UsuarioService : IUsuarioService
{
    private readonly ElTruequeDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;

    // Extensiones de imagen permitidas (celular/cámara/galería)
    private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".heic", ".heif" };
    private const long TamanoMaximoMB = 5; // 5 MB máximo

    public UsuarioService(ElTruequeDbContext db, IWebHostEnvironment env, IConfiguration config)
    {
        _db = db;
        _env = env;
        _config = config;
    }

    /// <summary>
    /// Actualiza la foto de perfil del usuario
    /// </summary>
    public async Task<UsuarioResponse?> ActualizarFotoPerfilAsync(int usuarioID, IFormFile imagen)
    {
        // Validar que el usuario exista
        var usuario = await _db.Usuarios
            .Include(u => u.Municipio)
                .ThenInclude(m => m!.Departamento)
            .FirstOrDefaultAsync(u => u.UsuarioID == usuarioID && u.EstadoActivo);

        if (usuario is null)
            return null;

        // Validar imagen
        var validacion = ValidarImagen(imagen);
        if (!validacion.esValida)
            throw new ArgumentException(validacion.mensaje);

        // Crear carpeta si no existe
        string carpetaFotos = Path.Combine(_env.WebRootPath, "fotos-perfil");
        Directory.CreateDirectory(carpetaFotos);

        // Eliminar foto anterior si existe
        if (!string.IsNullOrEmpty(usuario.FotoPerfil))
        {
            var rutaAnterior = Path.Combine(
                _env.WebRootPath,
                usuario.FotoPerfil.Replace("/", "\\").TrimStart('/', '\\')
            );
            if (File.Exists(rutaAnterior))
                File.Delete(rutaAnterior);
        }

        // Generar nombre único
        string extension = Path.GetExtension(Path.GetFileName(imagen.FileName)).ToLowerInvariant();
        string nombreArchivo = $"{usuarioID}_{Guid.NewGuid().ToString()[..8]}{extension}";
        string rutaCompleta = Path.Combine(carpetaFotos, nombreArchivo);

        // Guardar archivo
        using (var stream = new FileStream(rutaCompleta, FileMode.Create))
        {
            await imagen.CopyToAsync(stream);
        }

        // Actualizar URL en BD (ruta relativa)
        usuario.FotoPerfil = $"/fotos-perfil/{nombreArchivo}";

        _db.Usuarios.Update(usuario);
        await _db.SaveChangesAsync();

        return await ConstruirRespuestaAsync(usuario);
    }

    /// <summary>
    /// Actualiza datos del perfil del usuario
    /// </summary>
    public async Task<UsuarioResponse?> ActualizarPerfilAsync(int usuarioID, ActualizarPerfilRequest request)
    {
        var usuario = await _db.Usuarios
            .Include(u => u.Municipio)
                .ThenInclude(m => m!.Departamento)
            .FirstOrDefaultAsync(u => u.UsuarioID == usuarioID && u.EstadoActivo);

        if (usuario is null)
            return null;

        // Actualizar solo los campos que se proporcionen
        if (!string.IsNullOrWhiteSpace(request.Nombres))
            usuario.Nombres = request.Nombres;

        if (!string.IsNullOrWhiteSpace(request.Apellidos))
            usuario.Apellidos = request.Apellidos;

        if (!string.IsNullOrWhiteSpace(request.Telefono))
        {
            // Verificar que el teléfono no esté en uso por otro usuario
            bool telefonoEnUso = await _db.Usuarios
                .AnyAsync(u => u.Telefono == request.Telefono && u.UsuarioID != usuarioID);

            if (telefonoEnUso)
                throw new InvalidOperationException("El teléfono ya está registrado");

            usuario.Telefono = request.Telefono;
        }

        if (!string.IsNullOrWhiteSpace(request.Correo))
            usuario.Correo = request.Correo;

        if (!string.IsNullOrWhiteSpace(request.DireccionExacta))
            usuario.DireccionExacta = request.DireccionExacta;

        _db.Usuarios.Update(usuario);
        await _db.SaveChangesAsync();

        return await ConstruirRespuestaAsync(usuario);
    }

    /// <summary>
    /// Obtiene el perfil actual del usuario
    /// </summary>
    public async Task<UsuarioResponse?> ObtenerPerfilAsync(int usuarioID)
    {
        var usuario = await _db.Usuarios
            .Include(u => u.Municipio)
                .ThenInclude(m => m!.Departamento)
            .FirstOrDefaultAsync(u => u.UsuarioID == usuarioID && u.EstadoActivo);

        if (usuario is null)
            return null;

        return await ConstruirRespuestaAsync(usuario);
    }

    /// <summary>
    /// Construye la respuesta de usuario con valoración promedio
    /// </summary>
    private async Task<UsuarioResponse> ConstruirRespuestaAsync(Usuario usuario)
    {
        double promedio = await _db.Valoraciones
            .Where(v => v.UsuarioEvaluadoID == usuario.UsuarioID)
            .Select(v => (double?)v.Puntuacion)
            .AverageAsync() ?? 0;

        return new UsuarioResponse
        {
            UsuarioID = usuario.UsuarioID,
            Nombres = usuario.Nombres,
            Apellidos = usuario.Apellidos,
            Telefono = usuario.Telefono,
            Correo = usuario.Correo,
            Municipio = usuario.Municipio?.Nombre,
            Departamento = usuario.Municipio?.Departamento?.Nombre,
            DireccionExacta = usuario.DireccionExacta,
            EsProductora = usuario.EsProductora,
            FotoPerfil = usuario.FotoPerfil,
            PromedioValoracion = Math.Round(promedio, 1)
        };
    }

    /// <summary>
    /// Valida que la imagen sea válida
    /// </summary>
    private (bool esValida, string mensaje) ValidarImagen(IFormFile imagen)
    {
        if (imagen == null || imagen.Length == 0)
            return (false, "No se proporcionó imagen");

        // Validar extensión
        string extension = Path.GetExtension(Path.GetFileName(imagen.FileName)).ToLowerInvariant();
        if (!ExtensionesPermitidas.Contains(extension))
            return (false, $"Tipo de archivo no permitido. Extensiones válidas: {string.Join(", ", ExtensionesPermitidas)}");

        // Validar tamaño (5 MB máximo)
        if (imagen.Length > TamanoMaximoMB * 1024 * 1024)
            return (false, $"El archivo no puede exceder {TamanoMaximoMB} MB");

        // Validar que sea realmente una imagen (MIME type básico)
        if (!imagen.ContentType.StartsWith("image/"))
            return (false, "El archivo debe ser una imagen");

        return (true, "");
    }
}

