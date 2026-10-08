using System.ComponentModel.DataAnnotations;

namespace ElTrueque.Api.DTOs;

public class TruequeCreateRequest
{
    [Required]
    public int ProductoOfertadoID { get; set; }   // producto que YO tengo y ofrezco

    public int? ProductoSolicitadoID { get; set; } // producto que pido a cambio (null = venta directa)

    public decimal? MontoAdicional { get; set; }

    [MaxLength(200)]
    public string? LugarEncuentro { get; set; }
}

public class TruequeRespuestaRequest
{
    [Required]
    public string Accion { get; set; } = string.Empty; // "Aceptar" | "Rechazar"

    [MaxLength(200)]
    public string? LugarEncuentro { get; set; }
}

public class TruequeResponse
{
    public int TruequeID { get; set; }
    public string Estado { get; set; } = string.Empty;
    public bool YaValore { get; set; }

    public int ProductoOfertadoID { get; set; }
    public string ProductoOfertadoNombre { get; set; } = string.Empty;

    public int? ProductoSolicitadoID { get; set; }
    public string? ProductoSolicitadoNombre { get; set; }

    public int UsuarioSolicitanteID { get; set; }
    public string UsuarioSolicitanteNombre { get; set; } = string.Empty;

    public int UsuarioReceptorID { get; set; }
    public string UsuarioReceptorNombre { get; set; } = string.Empty;

    public decimal? MontoAdicional { get; set; }
    public string? LugarEncuentro { get; set; }

    public DateTime FechaSolicitud { get; set; }
    public DateTime? FechaRespuesta { get; set; }
    public DateTime? FechaCompletado { get; set; }
}

public class ValoracionCreateRequest
{
    [Required]
    public int TruequeID { get; set; }

    [Range(1, 5)]
    public byte Puntuacion { get; set; }

    [MaxLength(300)]
    public string? Comentario { get; set; }
}
