using ElTrueque.Api.DTOs;
using ElTrueque.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElTrueque.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly IProductoService _productoService;

    public ProductosController(IProductoService productoService)
    {
        _productoService = productoService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductoResponse>>> Listar([FromQuery] ProductoFiltro filtro)
    {
        var productos = await _productoService.ListarAsync(filtro);
        Response.Headers.Append("X-Total-Count", productos.Count.ToString());
        return Ok(productos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductoResponse>> Obtener(int id)
    {
        var producto = await _productoService.ObtenerAsync(id);
        return producto is null ? NotFound() : Ok(producto);
    }

    [Authorize]
    [Consumes("multipart/form-data")]
    [HttpPost]
    public async Task<ActionResult<ProductoResponse>> Crear([FromForm] ProductoCreateRequest request)
    {
        int usuarioID = User.GetUsuarioID();
        var creado = await _productoService.CrearAsync(usuarioID, request);
        return CreatedAtAction(nameof(Obtener), new { id = creado.ProductoID }, creado);
    }

    [Authorize]
    [Consumes("multipart/form-data")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromForm] ProductoUpdateRequest request)
    {
        int usuarioID = User.GetUsuarioID();
        var actualizado = await _productoService.ActualizarAsync(id, usuarioID, request);

        if (!actualizado)
        {
            var producto = await _productoService.ObtenerAsync(id);
            if (producto is null) return NotFound();
            return Forbid();
        }

        return NoContent();
    }

    [Authorize]
    [HttpPut("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] ProductoEstadoRequest request)
    {
        int usuarioID = User.GetUsuarioID();
        var resultado = await _productoService.CambiarEstadoAsync(id, usuarioID, request.Estado);

        return resultado switch
        {
            CambioEstadoProductoResultado.Actualizado => NoContent(),
            CambioEstadoProductoResultado.NoEncontrado => NotFound(),
            CambioEstadoProductoResultado.SinPermiso => Forbid(),
            _ => Conflict(new { mensaje = "Solo puedes cambiar entre Disponible e Inactivo cuando el producto no está reservado ni intercambiado." })
        };
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        int usuarioID = User.GetUsuarioID();
        var eliminado = await _productoService.EliminarAsync(id, usuarioID);

        if (!eliminado)
        {
            var producto = await _productoService.ObtenerAsync(id);
            if (producto is null) return NotFound();
            return Forbid();
        }

        return NoContent();
    }

    [Authorize]
    [HttpGet("mios")]
    public async Task<ActionResult<IEnumerable<ProductoResponse>>> Mios()
    {
        int usuarioID = User.GetUsuarioID();
        return Ok(await _productoService.MiosAsync(usuarioID));
    }
}
