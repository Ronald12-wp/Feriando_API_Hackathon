using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ElTrueque.Api.Data;
using ElTrueque.Api.DTOs;
using ElTrueque.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ElTrueque.Api.Services;

public interface IAuthService
{
    Task<AuthResponse?> RegistrarAsync(RegistroRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
}

public class AuthService : IAuthService
{
    private readonly ElTruequeDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(ElTruequeDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<AuthResponse?> RegistrarAsync(RegistroRequest request)
    {
        bool telefonoExiste = await _db.Usuarios.AnyAsync(u => u.Telefono == request.Telefono);
        if (telefonoExiste) return null; // El controller decide qué error mostrar

        var usuario = new Usuario
        {
            Nombres = request.Nombres,
            Apellidos = request.Apellidos,
            Telefono = request.Telefono,
            Correo = request.Correo,
            PasswordHash = Encoding.UTF8.GetBytes(BCrypt.Net.BCrypt.HashPassword(request.Password)),
            Genero = request.Genero,
            // --- CAMBIOS DE LOCALIZACIÓN ---
            MunicipioID = request.MunicipioID,
            DireccionExacta = request.DireccionExacta,
            // -------------------------------
            IdiomaPreferidoID = request.IdiomaPreferidoID,
            EsProductora = request.EsProductora,
            FechaRegistro = DateTime.UtcNow,
            EstadoActivo = true
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        return await ConstruirRespuestaAsync(usuario);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var usuario = await _db.Usuarios
            .Include(u => u.Municipio)
                .ThenInclude(m => m!.Departamento)
            .FirstOrDefaultAsync(u => u.Telefono == request.Telefono && u.EstadoActivo);

        if (usuario is null) return null;

        string hashGuardado = Encoding.UTF8.GetString(usuario.PasswordHash);
        bool passwordValida = BCrypt.Net.BCrypt.Verify(request.Password, hashGuardado);
        if (!passwordValida) return null;

        return await ConstruirRespuestaAsync(usuario);
    }

    private async Task<AuthResponse> ConstruirRespuestaAsync(Usuario usuario)
    {
        double promedio = await _db.Valoraciones
            .Where(v => v.UsuarioEvaluadoID == usuario.UsuarioID)
            .Select(v => (double?)v.Puntuacion)
            .AverageAsync() ?? 0;

        // Carga diferida en caso de que usuario no tenga precargado el municipio/departamento (como en el Registro)
        var municipio = usuario.Municipio ?? await _db.Municipios
            .Include(m => m.Departamento)
            .FirstOrDefaultAsync(m => m.MunicipioID == usuario.MunicipioID);

        int expiresInMinutes = int.Parse(_config["Jwt:ExpiresInMinutes"] ?? "1440");
        var expira = DateTime.UtcNow.AddMinutes(expiresInMinutes);

        var token = GenerarToken(usuario, expira);

        return new AuthResponse
        {
            Token = token,
            ExpiraEn = expira,
            Usuario = new UsuarioResponse
            {
                UsuarioID = usuario.UsuarioID,
                Nombres = usuario.Nombres,
                Apellidos = usuario.Apellidos,
                Telefono = usuario.Telefono,
                Correo = usuario.Correo,
                Genero = usuario.Genero,
                // --- CAMBIOS DE LOCALIZACIÓN ---
                MunicipioID = usuario.MunicipioID,
                Municipio = municipio?.Nombre,
                Departamento = municipio?.Departamento?.Nombre,
                DireccionExacta = usuario.DireccionExacta,
                IdiomaPreferidoID = usuario.IdiomaPreferidoID,
                // -------------------------------
                EsProductora = usuario.EsProductora,
                FotoPerfil = usuario.FotoPerfil,
                PromedioValoracion = Math.Round(promedio, 1)
            }
        };
    }

    private string GenerarToken(Usuario usuario, DateTime expira)
    {
        var claves = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credenciales = new SigningCredentials(claves, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.UsuarioID.ToString()),
            new(ClaimTypes.NameIdentifier, usuario.UsuarioID.ToString()),
            new(ClaimTypes.MobilePhone, usuario.Telefono),
            new("nombre", $"{usuario.Nombres} {usuario.Apellidos}")
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expira,
            signingCredentials: credenciales
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
