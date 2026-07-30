using ElTrueque.Api.Data;
using ElTrueque.Api.DTOs;
using ElTrueque.Api.Models;
using ElTrueque.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElTrueque.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly ElTruequeDbContext _db;

    public ProductosController(ElTruequeDbContext db)
    {
        _db = db;
    }

    // GET api/productos?categoriaId=1&municipioId=2&tipoOferta=Trueque&busqueda=miel
    // Público: cualquiera puede explorar el catálogo, no requiere sesión.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductoResponse>>> Listar([FromQuery] ProductoFiltro filtro)
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

        // --- FILTRO DE UBICACIÓN ACTUALIZADO ---
        if (filtro.MunicipioID.HasValue)
            query = query.Where(p => p.Usuario!.MunicipioID == filtro.MunicipioID.Value);

        if (filtro.DepartamentoID.HasValue)
            query = query.Where(p => p.Usuario!.Municipio!.DepartamentoID == filtro.DepartamentoID.Value);
        // ---------------------------------------

        if (!string.IsNullOrWhiteSpace(filtro.TipoOferta))
            query = query.Where(p => p.TipoOferta == filtro.TipoOferta || p.TipoOferta == "Ambos");

        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
            query = query.Where(p => p.Nombre.Contains(filtro.Busqueda) ||
                                      (p.Descripcion != null && p.Descripcion.Contains(filtro.Busqueda)));

        var total = await query.CountAsync();

        var productos = await query
            .OrderByDescending(p => p.FechaPublicacion)
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
            .Take(filtro.TamanoPagina)
            .Select(p => MapearRespuesta(p))
            .ToListAsync();

        Response.Headers.Append("X-Total-Count", total.ToString());
        return Ok(productos);
    }

    // GET api/productos/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductoResponse>> Obtener(int id)
    {
        var producto = await _db.Productos
            .Include(p => p.Usuario).ThenInclude(u => u!.Municipio).ThenInclude(m => m!.Departamento)
            .Include(p => p.Categoria)
            .Include(p => p.UnidadMedida)
            .Include(p => p.Imagenes)
            .FirstOrDefaultAsync(p => p.ProductoID == id);

        return producto is null ? NotFound() : Ok(MapearRespuesta(producto));
    }

    // POST api/productos (requiere sesión)
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ProductoResponse>> Crear(ProductoCreateRequest request)
    {
        int usuarioID = User.GetUsuarioID();

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

        var creado = await _db.Productos
            .Include(p => p.Usuario).ThenInclude(u => u!.Municipio).ThenInclude(m => m!.Departamento)
            .Include(p => p.Categoria)
            .Include(p => p.UnidadMedida)
            .Include(p => p.Imagenes)
            .FirstAsync(p => p.ProductoID == producto.ProductoID);

        return CreatedAtAction(nameof(Obtener), new { id = producto.ProductoID }, MapearRespuesta(creado));
    }

    // PUT api/productos/5 (solo la dueña del producto)
    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, ProductoUpdateRequest request)
    {
        int usuarioID = User.GetUsuarioID();
        var producto = await _db.Productos.FirstOrDefaultAsync(p => p.ProductoID == id);

        if (producto is null) return NotFound();
        if (producto.UsuarioID != usuarioID) return Forbid();

        producto.Nombre = request.Nombre ?? producto.Nombre;
        producto.Descripcion = request.Descripcion ?? producto.Descripcion;
        producto.Cantidad = request.Cantidad ?? producto.Cantidad;
        producto.TipoOferta = request.TipoOferta ?? producto.TipoOferta;
        producto.PrecioReferencial = request.PrecioReferencial ?? producto.PrecioReferencial;
        producto.Estado = request.Estado ?? producto.Estado;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE api/productos/5 (solo la dueña del producto)
    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        int usuarioID = User.GetUsuarioID();
        var producto = await _db.Productos.FirstOrDefaultAsync(p => p.ProductoID == id);

        if (producto is null) return NotFound();
        if (producto.UsuarioID != usuarioID) return Forbid();

        _db.Productos.Remove(producto);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // GET api/productos/mios (los productos publicados por la usuaria autenticada)
    [Authorize]
    [HttpGet("mios")]
    public async Task<ActionResult<IEnumerable<ProductoResponse>>> Mios()
    {
        int usuarioID = User.GetUsuarioID();

        var productos = await _db.Productos
            .Include(p => p.Usuario).ThenInclude(u => u!.Municipio).ThenInclude(m => m!.Departamento)
            .Include(p => p.Categoria)
            .Include(p => p.UnidadMedida)
            .Include(p => p.Imagenes)
            .Where(p => p.UsuarioID == usuarioID)
            .OrderByDescending(p => p.FechaPublicacion)
            .Select(p => MapearRespuesta(p))
            .ToListAsync();

        return Ok(productos);
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
        // --- PROPIEDADES DE LOCALIZACIÓN ACTUALIZADAS ---
        Municipio = p.Usuario?.Municipio?.Nombre ?? string.Empty,
        Departamento = p.Usuario?.Municipio?.Departamento?.Nombre ?? string.Empty
    };
}