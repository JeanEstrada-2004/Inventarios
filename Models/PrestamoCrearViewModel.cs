using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Inventarios.Models;

public class PrestamoCrearViewModel
{
    [Required]
    [Display(Name = "Herramienta")]
    public Guid HerramientaId { get; set; }

    [Required]
    [Display(Name = "Estante")]
    public string Estante { get; set; } = "Estante A";

    [Range(1, 100)]
    [Display(Name = "Cantidad")]
    public int Cantidad { get; set; } = 1;

    public List<SelectListItem> Estantes { get; set; } = new();
    public List<SelectListItem> HerramientasDisponibles { get; set; } = new();
    public int DisponiblesHerramienta { get; set; }
}
