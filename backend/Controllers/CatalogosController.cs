using ElTrueque.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElTrueque.Api.Controllers;

// Endpoints públicos de solo lectura: alimentan los combos/selects de la app
// (departamento, municipio, categoría, unidad de medida, idioma) al registrarse o publicar un producto.
[ApiController]
[Route("api/[controller]")]
public class CatalogosController : ControllerBase
{
    private readonly ElTruequeDbContext _db;

    public CatalogosController(ElTruequeDbContext db)
    {
        _db = db;
    }

    [HttpGet("departamentos")]
    public async Task<IActionResult> Departamentos()
    {
        var departamentos = await _db.Departamentos
            .OrderBy(d => d.Nombre)
            .Select(d => new
            {
                d.DepartamentoID,
                d.Nombre,
                Municipios = d.Municipios
                    .OrderBy(m => m.Nombre)
                    .Select(m => new
                    {
                        m.MunicipioID,
                        m.Nombre
                    })
                    .ToList()
            })
            .ToListAsync();

        return Ok(departamentos);
    }

    [HttpGet("municipios")]
    public async Task<IActionResult> Municipios([FromQuery] int? departamentoId)
    {
        var query = _db.Municipios.Include(m => m.Departamento).AsQueryable();

        if (departamentoId.HasValue)
        {
            query = query.Where(m => m.DepartamentoID == departamentoId.Value);
        }

        var municipios = await query
            .OrderBy(m => m.Nombre)
            .Select(m => new
            {
                m.MunicipioID,
                m.Nombre,
                m.DepartamentoID,
                Departamento = m.Departamento!.Nombre
            })
            .ToListAsync();

        return Ok(municipios);
    }

    [HttpGet("categorias")]
    public async Task<IActionResult> Categorias() =>
        Ok(await _db.Categorias.OrderBy(c => c.Nombre).ToListAsync());

    [HttpGet("unidades-medida")]
    public async Task<IActionResult> UnidadesMedida() =>
        Ok(await _db.UnidadesMedida.OrderBy(u => u.Nombre).ToListAsync());

    [HttpGet("idiomas")]
    public async Task<IActionResult> Idiomas() =>
        Ok(await _db.Idiomas.OrderBy(i => i.Nombre).ToListAsync());
}