using Inventarios.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Inventarios.Data;

// DbContext principal para la aplicación, incluye Identity y las entidades del inventario.
public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Herramienta> Herramientas => Set<Herramienta>();
    public DbSet<HerramientaUnidad> HerramientasUnidades => Set<HerramientaUnidad>();
    public DbSet<Prestamo> Prestamos => Set<Prestamo>();
    public DbSet<PrestamoDetalle> PrestamosDetalles => Set<PrestamoDetalle>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<HerramientaUnidad>()
            .Property(h => h.Estado)
            .HasConversion<string>();

        builder.Entity<Prestamo>()
            .Property(p => p.Estado)
            .HasConversion<string>();

        builder.Entity<HerramientaUnidad>()
            .HasOne(h => h.Herramienta)
            .WithMany(h => h.Unidades)
            .HasForeignKey(h => h.HerramientaId);

        builder.Entity<PrestamoDetalle>()
            .HasOne(d => d.Prestamo)
            .WithMany(p => p.Detalles)
            .HasForeignKey(d => d.PrestamoId);

        builder.Entity<PrestamoDetalle>()
            .HasOne(d => d.HerramientaUnidad)
            .WithMany(u => u.PrestamosDetalles)
            .HasForeignKey(d => d.HerramientaUnidadId);
    }
}
