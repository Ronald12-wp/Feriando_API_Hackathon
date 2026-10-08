using ElTrueque.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ElTrueque.Api.Data;

public class ElTruequeDbContext : DbContext
{
    public ElTruequeDbContext(DbContextOptions<ElTruequeDbContext> options) : base(options) { }

    public DbSet<Departamento> Departamentos => Set<Departamento>();
    public DbSet<Municipio> Municipios => Set<Municipio>();
    public DbSet<Idioma> Idiomas => Set<Idioma>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<UnidadMedida> UnidadesMedida => Set<UnidadMedida>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<ImagenProducto> ImagenesProducto => Set<ImagenProducto>();
    public DbSet<Trueque> Trueques => Set<Trueque>();
    public DbSet<Valoracion> Valoraciones => Set<Valoracion>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
    public DbSet<MensajeChat> MensajesChat { get; set; }
    public DbSet<ConversacionOculta> ConversacionesOcultas => Set<ConversacionOculta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Evita ciclos de borrado en cascada entre Trueques y Usuarios/Productos
        // (SQL Server no permite múltiples rutas de cascada hacia la misma tabla)
        modelBuilder.Entity<Trueque>()
            .HasOne(t => t.ProductoOfertado)
            .WithMany()
            .HasForeignKey(t => t.ProductoOfertadoID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Trueque>()
            .HasOne(t => t.ProductoSolicitado)
            .WithMany()
            .HasForeignKey(t => t.ProductoSolicitadoID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Trueque>()
            .HasOne(t => t.UsuarioSolicitante)
            .WithMany()
            .HasForeignKey(t => t.UsuarioSolicitanteID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Trueque>()
            .HasOne(t => t.UsuarioReceptor)
            .WithMany()
            .HasForeignKey(t => t.UsuarioReceptorID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Valoracion>()
            .HasOne(v => v.UsuarioEvaluador)
            .WithMany()
            .HasForeignKey(v => v.UsuarioEvaluadorID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Valoracion>()
            .HasOne(v => v.UsuarioEvaluado)
            .WithMany()
            .HasForeignKey(v => v.UsuarioEvaluadoID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Telefono)
            .IsUnique();

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Cedula)
            .IsUnique()
            .HasFilter("[Cedula] IS NOT NULL");

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Correo)
            .IsUnique()
            .HasFilter("[Correo] IS NOT NULL");

        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Departamento)
            .WithMany()
            .HasForeignKey(u => u.DepartamentoID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Producto>()
            .Property(p => p.Estado)
            .HasDefaultValue("Disponible");

        modelBuilder.Entity<Producto>()
            .ToTable(table => table.HasCheckConstraint(
                "CK_Productos_Estado",
                "[Estado] IN (N'Disponible', N'Reservado', N'Intercambiado', N'Inactivo')"));

        modelBuilder.Entity<Trueque>()
            .Property(t => t.Estado)
            .HasDefaultValue("Pendiente");

        modelBuilder.Entity<Trueque>()
            .ToTable(table => table.HasCheckConstraint(
                "CK_Trueques_Estado",
                "[Estado] IN (N'Pendiente', N'Aceptado', N'Rechazado', N'Completado', N'Cancelado')"));

        modelBuilder.Entity<Valoracion>()
            .HasIndex(v => new { v.TruequeID, v.UsuarioEvaluadorID })
            .IsUnique();

        modelBuilder.Entity<ConversacionOculta>()
            .HasKey(c => new { c.UsuarioID, c.ChatId });

        modelBuilder.Entity<ConversacionOculta>()
            .HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(c => c.UsuarioID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
