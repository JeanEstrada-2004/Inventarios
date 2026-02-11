using System.ComponentModel.DataAnnotations;

namespace Inventarios.Models;

public class PrestamoDetalle
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid PrestamoId { get; set; }
    public Prestamo? Prestamo { get; set; }

    [Required]
    public Guid HerramientaUnidadId { get; set; }
    public HerramientaUnidad? HerramientaUnidad { get; set; }
}
