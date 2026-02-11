using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Inventarios.Models;

public enum EstadoPrestamo
{
    Activo,
    DevueltoPendiente,
    Cerrado
}

public class Prestamo
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string UsuarioId { get; set; } = string.Empty;

    public IdentityUser? Usuario { get; set; }

    public DateTime FechaPrestamo { get; set; } = DateTime.UtcNow;

    public DateTime? FechaDevolucion { get; set; }

    [Required]
    public EstadoPrestamo Estado { get; set; } = EstadoPrestamo.Activo;

    public ICollection<PrestamoDetalle> Detalles { get; set; } = new List<PrestamoDetalle>();
}
