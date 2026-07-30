using System.ComponentModel.DataAnnotations;

namespace ElTrueque.Api.DTOs;

public class RegistroRequest
{
    [Required, MaxLength(100)]
    public string Nombres { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Apellidos { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Telefono { get; set; } = string.Empty;

    [MaxLength(150), EmailAddress]
    public string? Correo { get; set; }

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    public string? Genero { get; set; }

    // --- CAMBIOS DE LOCALIZACIÓN ---
    [Required]
    public int MunicipioID { get; set; }

    [Required, MaxLength(300)]
    public string DireccionExacta { get; set; } = string.Empty;
    // -------------------------------

    public int? IdiomaPreferidoID { get; set; }

    public bool EsProductora { get; set; } = true;
}

public class LoginRequest
{
    [Required]
    public string Telefono { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiraEn { get; set; }
    public UsuarioResponse Usuario { get; set; } = null!;
}

public class UsuarioResponse
{
    public int UsuarioID { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Correo { get; set; }

    // --- CAMBIOS DE LOCALIZACIÓN ---
    public string? Municipio { get; set; }
    public string? Departamento { get; set; }
    public string DireccionExacta { get; set; } = string.Empty;
    // -------------------------------

    public bool EsProductora { get; set; }
    public string? FotoPerfil { get; set; }
    public double PromedioValoracion { get; set; }
}