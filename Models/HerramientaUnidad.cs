using System.ComponentModel.DataAnnotations;

namespace Inventarios.Models;

public enum EstadoHerramientaUnidad
{
    Disponible,
    Prestada,
    Danada
}

public class HerramientaUnidad
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid HerramientaId { get; set; }
    public Herramienta? Herramienta { get; set; }

    [Required]
    [StringLength(50)]
    public string Etiqueta { get; set; } = "Unidad";

    [Required]
    public EstadoHerramientaUnidad Estado { get; set; } = EstadoHerramientaUnidad.Disponible;

    [StringLength(200)]
    public string? Observacion { get; set; }

    public ICollection<PrestamoDetalle> PrestamosDetalles { get; set; } = new List<PrestamoDetalle>();
}
