using ElTrueque.Api.DTOs;
using ElTrueque.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElTrueque.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TruequesController : ControllerBase
{
    private readonly ITruequeService _truequeService;

    public TruequesController(ITruequeService truequeService)
    {
        _truequeService = truequeService;
    }

    // GET api/trueques/mios  -> todas las solicitudes donde participo (enviadas o recibidas)
    [HttpGet("mios")]
    public async Task<ActionResult<IEnumerable<TruequeResponse>>> Mios()
    {
        int usuarioID = User.GetUsuarioID();
        return Ok(await _truequeService.ListarPorUsuarioAsync(usuarioID));
    }

    // POST api/trueques  -> solicitar un intercambio o compra
    [HttpPost]
    public async Task<ActionResult<TruequeResponse>> Solicitar(TruequeCreateRequest request)
    {
        int usuarioID = User.GetUsuarioID();
        var (exito, mensaje, trueque) = await _truequeService.SolicitarAsync(usuarioID, request);

        if (!exito) return BadRequest(new { mensaje });
        return CreatedAtAction(nameof(Mios), trueque);
    }

    // PUT api/trueques/5/responder  -> Aceptar o Rechazar
    [HttpPut("{id:int}/responder")]
    public async Task<ActionResult<TruequeResponse>> Responder(int id, TruequeRespuestaRequest request)
    {
        int usuarioID = User.GetUsuarioID();
        var (exito, mensaje, trueque) = await _truequeService.ResponderAsync(usuarioID, id, request);

        if (!exito) return BadRequest(new { mensaje });
        return Ok(trueque);
    }
}
