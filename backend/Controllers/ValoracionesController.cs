using ElTrueque.Api.Data;
using ElTrueque.Api.DTOs;
using ElTrueque.Api.Models;
using ElTrueque.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ElTrueque.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ValoracionesController : ControllerBase
{
    private readonly ElTruequeDbContext _db;

    public ValoracionesController(ElTruequeDbContext db)
    {
        _db = db;
    }

    // POST api/valoraciones -> cada participante valora; la segunda valoración completa el trueque.
    [HttpPost]
    public async Task<IActionResult> Crear(ValoracionCreateRequest request)
    {
        int usuarioID = User.GetUsuarioID();

        await using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var trueque = await _db.Trueques
            .Include(t => t.ProductoOfertado)
            .Include(t => t.ProductoSolicitado)
            .FirstOrDefaultAsync(t => t.TruequeID == request.TruequeID);
        if (trueque is null) return NotFound(new { mensaje = "Trueque no encontrado." });

        bool participante = trueque.UsuarioSolicitanteID == usuarioID || trueque.UsuarioReceptorID == usuarioID;
        if (!participante) return Forbid();

        if (trueque.Estado != "Aceptado")
            return BadRequest(new { mensaje = "Solo puedes valorar un trueque aceptado que aún no haya sido completado." });

        int usuarioEvaluadoID = trueque.UsuarioSolicitanteID == usuarioID
            ? trueque.UsuarioReceptorID
            : trueque.UsuarioSolicitanteID;

        bool yaValoro = await _db.Valoraciones.AnyAsync(v =>
            v.TruequeID == request.TruequeID && v.UsuarioEvaluadorID == usuarioID);
        if (yaValoro) return Conflict(new { mensaje = "Ya valoraste este intercambio." });

        _db.Valoraciones.Add(new Valoracion
        {
            TruequeID = request.TruequeID,
            UsuarioEvaluadorID = usuarioID,
            UsuarioEvaluadoID = usuarioEvaluadoID,
            Puntuacion = request.Puntuacion,
            Comentario = request.Comentario,
            FechaValoracion = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        var cantidadValoraciones = await _db.Valoraciones
            .CountAsync(v => v.TruequeID == request.TruequeID);

        if (cantidadValoraciones >= 2)
        {
            trueque.Estado = "Completado";
            trueque.FechaCompletado = DateTime.UtcNow;
            trueque.ProductoOfertado!.Estado = "Intercambiado";
            if (trueque.ProductoSolicitado is not null)
                trueque.ProductoSolicitado.Estado = "Intercambiado";

            await _db.SaveChangesAsync();
        }

        await transaction.CommitAsync();
        return Ok(new { mensaje = "Valoración registrada." });
    }

    // GET api/valoraciones/usuario/5
    [AllowAnonymous]
    [HttpGet("usuario/{usuarioId:int}")]
    public async Task<IActionResult> PorUsuario(int usuarioId)
    {
        var valoraciones = await _db.Valoraciones
            .Where(v => v.UsuarioEvaluadoID == usuarioId)
            .OrderByDescending(v => v.FechaValoracion)
            .Select(v => new
            {
                v.ValoracionID,
                v.Puntuacion,
                v.Comentario,
                v.FechaValoracion
            })
            .ToListAsync();

        return Ok(valoraciones);
    }
}
