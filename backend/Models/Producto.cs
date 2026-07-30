using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElTrueque.Api.Models;

public class Producto
{
    [Key]
    public int ProductoID { get; set; }

    [Required]
    public int UsuarioID { get; set; }

    [ForeignKey(nameof(UsuarioID))]
    public Usuario? Usuario { get; set; }

    [Required]
    public int CategoriaID { get; set; }

    [ForeignKey(nameof(CategoriaID))]
    public Categoria? Categoria { get; set; }

    [Required, MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Cantidad { get; set; } = 1;

    [Required]
    public int UnidadMedidaID { get; set; }

    [ForeignKey(nameof(UnidadMedidaID))]
    public UnidadMedida? UnidadMedida { get; set; }

    [Required, MaxLength(20)]
    public string TipoOferta { get; set; } = "Trueque"; // Trueque | Venta | Ambos

    [Column(TypeName = "decimal(10,2)")]
    public decimal? PrecioReferencial { get; set; }

    [Required, MaxLength(20)]
    public string Estado { get; set; } = "Disponible"; // Disponible | Reservado | Intercambiado | Inactivo

    public DateTime FechaPublicacion { get; set; } = DateTime.UtcNow;

    public ICollection<ImagenProducto> Imagenes { get; set; } = new List<ImagenProducto>();
}

public class ImagenProducto
{
    [Key]
    public int ImagenID { get; set; }

    [Required]
    public int ProductoID { get; set; }

    [ForeignKey(nameof(ProductoID))]
    public Producto? Producto { get; set; }

    [Required, MaxLength(300)]
    public string UrlImagen { get; set; } = string.Empty;

    public byte Orden { get; set; } = 1;
}
