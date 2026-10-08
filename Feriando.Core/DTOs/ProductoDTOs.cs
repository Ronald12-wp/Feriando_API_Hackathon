using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ElTrueque.Api.DTOs;

public class ProductoCreateRequest
{
    [Required]
    public int CategoriaID { get; set; }

    [Required, MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Cantidad { get; set; } = 1;

    [Required]
    public int UnidadMedidaID { get; set; }

    [Required]
    public string TipoOferta { get; set; } = "Trueque"; // Trueque | Venta | Ambos

    public decimal? PrecioReferencial { get; set; }

    public List<string>? UrlsImagenes { get; set; }

    public List<IFormFile>? ImagenesArchivos { get; set; }
}

public class ProductoUpdateRequest
{
    public int? CategoriaID { get; set; }
    public int? UnidadMedidaID { get; set; }
    public string? Nombre { get; set; }
    public string? Descripcion { get; set; }
    public decimal? Cantidad { get; set; }
    public string? TipoOferta { get; set; }
    public decimal? PrecioReferencial { get; set; }
    public bool ReemplazarImagenes { get; set; }
    public List<IFormFile>? ImagenesArchivos { get; set; }
}

public class ProductoEstadoRequest
{
    [Required, MaxLength(20)]
    public string Estado { get; set; } = string.Empty;
}

public class ProductoResponse
{
    public int ProductoID { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Cantidad { get; set; }
    public string UnidadMedida { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string TipoOferta { get; set; } = string.Empty;
    public decimal? PrecioReferencial { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaPublicacion { get; set; }
    public List<string> Imagenes { get; set; } = new();

    public int UsuarioID { get; set; }
    public string NombreProductora { get; set; } = string.Empty;
    public string Comunidad { get; set; } = string.Empty;
    // Añadido: propiedades de localización que usa el controlador
    public string Municipio { get; set; } = string.Empty;
    public string Departamento { get; set; } = string.Empty;
}

public class ProductoFiltro
{
    public int? CategoriaID { get; set; }
    public int? ComunidadID { get; set; }
    // Añadido: filtros de ubicación usados por el controlador
    public int? MunicipioID { get; set; }
    public int? DepartamentoID { get; set; }
    public string? TipoOferta { get; set; }
    public string? Estado { get; set; } = "Disponible";
    public string? Busqueda { get; set; } // texto libre sobre Nombre/Descripcion
    public int Pagina { get; set; } = 1;
    public int TamanoPagina { get; set; } = 20;
}
