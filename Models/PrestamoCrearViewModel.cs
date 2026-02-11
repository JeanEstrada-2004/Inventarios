using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Inventarios.Models;

public class PrestamoCrearViewModel
{
    [Required]
    [Display(Name = "Herramienta")]
    public Guid HerramientaId { get; set; }

    public List<SelectListItem> HerramientasDisponibles { get; set; } = new();
}
