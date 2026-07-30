using System.Security.Claims;

namespace ElTrueque.Api.Services;

public static class ClaimsPrincipalExtensions
{
    public static int GetUsuarioID(this ClaimsPrincipal usuario)
    {
        var valor = usuario.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(valor, out var id) ? id : 0;
    }
}
