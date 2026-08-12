using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElTrueque.Api.Models;

public class Trueque
{
    [Key]
    public int TruequeID { get; set; }

    [Required]
    public int ProductoOfertadoID { get; set; }

    [ForeignKey(nameof(ProductoOfertadoID))]
    public Producto? ProductoOfertado { get; set; }

    public int? ProductoSolicitadoID { get; set; }

    [ForeignKey(nameof(ProductoSolicitadoID))]
    public Producto? ProductoSolicitado { get; set; }

    [Required]
    public int UsuarioSolicitanteID { get; set; }

    [ForeignKey(nameof(UsuarioSolicitanteID))]
    public Usuario? UsuarioSolicitante { get; set; }

    [Required]
    public int UsuarioReceptorID { get; set; }

    [ForeignKey(nameof(UsuarioReceptorID))]
    public Usuario? UsuarioReceptor { get; set; }

    [Required, MaxLength(20)]
    public string Estado { get; set; } = "Pendiente"; // Pendiente | Aceptado | Rechazado | Completado | Cancelado

    [Column(TypeName = "decimal(10,2)")]
    public decimal? MontoAdicional { get; set; } = 0;

    public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;
    public DateTime? FechaRespuesta { get; set; }
    public DateTime? FechaCompletado { get; set; }

    [MaxLength(200)]
    public string? LugarEncuentro { get; set; }

    public ICollection<Valoracion> Valoraciones { get; set; } = new List<Valoracion>();
}

public class Valoracion
{
    [Key]
    public int ValoracionID { get; set; }

    [Required]
    public int TruequeID { get; set; }

    [ForeignKey(nameof(TruequeID))]
    public Trueque? Trueque { get; set; }

    [Required]
    public int UsuarioEvaluadorID { get; set; }

    [ForeignKey(nameof(UsuarioEvaluadorID))]
    public Usuario? UsuarioEvaluador { get; set; }

    [Required]
    public int UsuarioEvaluadoID { get; set; }

    [ForeignKey(nameof(UsuarioEvaluadoID))]
    public Usuario? UsuarioEvaluado { get; set; }

    [Range(1, 5)]
    public byte Puntuacion { get; set; }

    [MaxLength(300)]
    public string? Comentario { get; set; }

    public DateTime FechaValoracion { get; set; } = DateTime.UtcNow;
}

public class Notificacion
{
    [Key]
    public int NotificacionID { get; set; }

    [Required]
    public int UsuarioID { get; set; }

    [ForeignKey(nameof(UsuarioID))]
    public Usuario? Usuario { get; set; }

    [Required, MaxLength(100)]
    public string Titulo { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string Mensaje { get; set; } = string.Empty;

    public int? TruequeID { get; set; }

    [ForeignKey(nameof(TruequeID))]
    public Trueque? Trueque { get; set; }

    public bool Leido { get; set; } = false;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
