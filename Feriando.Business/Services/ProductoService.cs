using Microsoft.AspNetCore.Http;
using ElTrueque.Api.Data;
using ElTrueque.Api.DTOs;
using ElTrueque.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ElTrueque.Api.Services;

public interface IProductoService
{
    Task<List<ProductoResponse>> ListarAsync(ProductoFiltro filtro);
    Task<ProductoResponse?> ObtenerAsync(int id);
    Task<ProductoResponse> CrearAsync(int usuarioID, ProductoCreateRequest request);
    Task<bool> ActualizarAsync(int id, int usuarioID, ProductoUpdateRequest request);
    Task<CambioEstadoProductoResultado> CambiarEstadoAsync(int id, int usuarioID, string estado);
    Task<bool> EliminarAsync(int id, int usuarioID);
    Task<List<ProductoResponse>> MiosAsync(int usuarioID);
}

public enum CambioEstadoProductoResultado
{
    Actualizado,
    NoEncontrado,
    SinPermiso,
    TransicionInvalida
}

public class ProductoService : IProductoService
{
    private readonly ElTruequeDbContext _db;
    private readonly IWebHostEnvironment _env;

    public ProductoService(ElTruequeDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public async Task<List<ProductoResponse>> ListarAsync(ProductoFiltro filtro)
    {
        var query = _db.Productos
            .Include(p => p.Usuario).ThenInclude(u => u!.Municipio).ThenInclude(m => m!.Departamento)
            .Include(p => p.Categoria)
            .Include(p => p.UnidadMedida)
            .Include(p => p.Imagenes)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro.Estado))
            query = query.Where(p => p.Estado == filtro.Estado);

        if (filtro.CategoriaID.HasValue)
            query = query.Where(p => p.CategoriaID == filtro.CategoriaID.Value);

        if (filtro.MunicipioID.HasValue)
            query = query.Where(p => p.Usuario!.MunicipioID == filtro.MunicipioID.Value);

        if (filtro.DepartamentoID.HasValue)
            query = query.Where(p => p.Usuario!.Municipio!.DepartamentoID == filtro.DepartamentoID.Value);

        if (!string.IsNullOrWhiteSpace(filtro.TipoOferta))
            query = query.Where(p => p.TipoOferta == filtro.TipoOferta || p.TipoOferta == "Ambos");

        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
            query = query.Where(p => p.Nombre.Contains(filtro.Busqueda) ||
                                      (p.Descripcion != null && p.Descripcion.Contains(filtro.Busqueda)));

        return await query
            .OrderByDescending(p => p.FechaPublicacion)
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
            .Take(filtro.TamanoPagina)
            .Select(p => MapearRespuesta(p))
            .ToListAsync();
    }

    public async Task<ProductoResponse?> ObtenerAsync(int id)
    {
        var producto = await _db.Productos
            .Include(p => p.Usuario).ThenInclude(u => u!.Municipio).ThenInclude(m => m!.Departamento)
            .Include(p => p.Categoria)
            .Include(p => p.UnidadMedida)
            .Include(p => p.Imagenes)
            .FirstOrDefaultAsync(p => p.ProductoID == id);

        return producto is null ? null : MapearRespuesta(producto);
    }

    public async Task<ProductoResponse> CrearAsync(int usuarioID, ProductoCreateRequest request)
    {
        var producto = new Producto
        {
            UsuarioID = usuarioID,
            CategoriaID = request.CategoriaID,
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Cantidad = request.Cantidad,
            UnidadMedidaID = request.UnidadMedidaID,
            TipoOferta = request.TipoOferta,
            PrecioReferencial = request.PrecioReferencial,
            Estado = "Disponible",
            FechaPublicacion = DateTime.UtcNow
        };

        if (request.UrlsImagenes is { Count: > 0 })
        {
            byte orden = 1;
            foreach (var url in request.UrlsImagenes)
                producto.Imagenes.Add(new ImagenProducto { UrlImagen = url, Orden = orden++ });
        }

        _db.Productos.Add(producto);
        await _db.SaveChangesAsync();

        await GuardarImagenesAsync(producto.ProductoID, request.ImagenesArchivos);
        await _db.SaveChangesAsync();

        var creado = await _db.Productos
            .Include(p => p.Usuario).ThenInclude(u => u!.Municipio).ThenInclude(m => m!.Departamento)
            .Include(p => p.Categoria)
            .Include(p => p.UnidadMedida)
            .Include(p => p.Imagenes)
            .FirstAsync(p => p.ProductoID == producto.ProductoID);

        return MapearRespuesta(creado);
    }

    public async Task<bool> ActualizarAsync(int id, int usuarioID, ProductoUpdateRequest request)
    {
        var producto = await _db.Productos
            .Include(p => p.Imagenes)
            .FirstOrDefaultAsync(p => p.ProductoID == id);

        if (producto is null)
            return false;

        if (producto.UsuarioID != usuarioID)
            return false;

        if (request.CategoriaID.HasValue)
            producto.CategoriaID = request.CategoriaID.Value;

        if (request.UnidadMedidaID.HasValue)
            producto.UnidadMedidaID = request.UnidadMedidaID.Value;

        producto.Nombre = request.Nombre ?? producto.Nombre;
        producto.Descripcion = request.Descripcion ?? producto.Descripcion;
        producto.Cantidad = request.Cantidad ?? producto.Cantidad;
        producto.TipoOferta = request.TipoOferta ?? producto.TipoOferta;
        producto.PrecioReferencial = request.PrecioReferencial ?? producto.PrecioReferencial;
        if (request.ReemplazarImagenes || request.ImagenesArchivos is { Count: > 0 })
        {
            await EliminarImagenesAsync(producto.ProductoID);
            await GuardarImagenesAsync(producto.ProductoID, request.ImagenesArchivos);
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<CambioEstadoProductoResultado> CambiarEstadoAsync(int id, int usuarioID, string estado)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var producto = await _db.Productos.FirstOrDefaultAsync(p => p.ProductoID == id);
        if (producto is null)
            return CambioEstadoProductoResultado.NoEncontrado;

        if (producto.UsuarioID != usuarioID)
            return CambioEstadoProductoResultado.SinPermiso;

        var transicionPermitida =
            (producto.Estado == "Disponible" && estado == "Inactivo") ||
            (producto.Estado == "Inactivo" && estado == "Disponible");

        if (!transicionPermitida)
            return CambioEstadoProductoResultado.TransicionInvalida;

        producto.Estado = estado;
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        return CambioEstadoProductoResultado.Actualizado;
    }

    public async Task<bool> EliminarAsync(int id, int usuarioID)
    {
        var producto = await _db.Productos.FirstOrDefaultAsync(p => p.ProductoID == id);
        if (producto is null)
            return false;

        if (producto.UsuarioID != usuarioID)
            return false;

        _db.Productos.Remove(producto);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<ProductoResponse>> MiosAsync(int usuarioID)
    {
        return await _db.Productos
            .Include(p => p.Usuario).ThenInclude(u => u!.Municipio).ThenInclude(m => m!.Departamento)
            .Include(p => p.Categoria)
            .Include(p => p.UnidadMedida)
            .Include(p => p.Imagenes)
            .Where(p => p.UsuarioID == usuarioID)
            .OrderByDescending(p => p.FechaPublicacion)
            .Select(p => MapearRespuesta(p))
            .ToListAsync();
    }

    private async Task EliminarImagenesAsync(int productoId)
    {
        var producto = await _db.Productos
            .Include(p => p.Imagenes)
            .FirstOrDefaultAsync(p => p.ProductoID == productoId);

        if (producto is null || producto.Imagenes.Count == 0)
            return;

        var carpeta = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "productos", productoId.ToString());
        if (Directory.Exists(carpeta))
        {
            foreach (var archivo in Directory.GetFiles(carpeta))
            {
                File.Delete(archivo);
            }
        }

        _db.ImagenesProducto.RemoveRange(producto.Imagenes);
        await _db.SaveChangesAsync();
    }

    private async Task GuardarImagenesAsync(int productoId, List<IFormFile>? imagenesArchivos)
    {
        if (imagenesArchivos is null || imagenesArchivos.Count == 0)
            return;

        var carpeta = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "productos", productoId.ToString());
        Directory.CreateDirectory(carpeta);

        byte orden = 1;
        var producto = await _db.Productos
            .Include(p => p.Imagenes)
            .FirstOrDefaultAsync(p => p.ProductoID == productoId);

        if (producto is null)
            return;

        foreach (var archivo in imagenesArchivos)
        {
            if (archivo.Length == 0)
                continue;

            var nombreArchivo = $"{Guid.NewGuid():N}_{Path.GetFileName(archivo.FileName)}";
            var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

            await using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            var url = $"/uploads/productos/{productoId}/{nombreArchivo}";
            producto.Imagenes.Add(new ImagenProducto
            {
                UrlImagen = url,
                Orden = orden++
            });
        }
    }

    private static ProductoResponse MapearRespuesta(Producto p) => new()
    {
        ProductoID = p.ProductoID,
        Nombre = p.Nombre,
        Descripcion = p.Descripcion,
        Cantidad = p.Cantidad,
        UnidadMedida = p.UnidadMedida?.Nombre ?? string.Empty,
        Categoria = p.Categoria?.Nombre ?? string.Empty,
        TipoOferta = p.TipoOferta,
        PrecioReferencial = p.PrecioReferencial,
        Estado = p.Estado,
        FechaPublicacion = p.FechaPublicacion,
        Imagenes = p.Imagenes.OrderBy(i => i.Orden).Select(i => i.UrlImagen).ToList(),
        UsuarioID = p.UsuarioID,
        NombreProductora = p.Usuario is null ? string.Empty : $"{p.Usuario.Nombres} {p.Usuario.Apellidos}",
        Municipio = p.Usuario?.Municipio?.Nombre ?? string.Empty,
        Departamento = p.Usuario?.Municipio?.Departamento?.Nombre ?? string.Empty
    };
}
