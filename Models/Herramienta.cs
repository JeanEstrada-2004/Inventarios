using System.ComponentModel.DataAnnotations;

namespace Inventarios.Models;

public class Herramienta
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, StringLength(50)]
    public string Codigo { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(60)]
    public string? Marca { get; set; }

    [Required, StringLength(20)]
    public string Estante { get; set; } = "Estante A";

    [Range(0, int.MaxValue)]
    public int CantidadTotal { get; set; }

    [Range(0, int.MaxValue)]
    public int CantidadDisponible { get; set; }

    [StringLength(200)]
    public string? Especificacion { get; set; }

    [StringLength(200)]
    public string? Observaciones { get; set; }

    public ICollection<HerramientaUnidad> Unidades { get; set; } = new List<HerramientaUnidad>();
}
