using System.ComponentModel.DataAnnotations;

namespace Inventarios.Models;

public class PerfilEditarViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Correo")]
    public string Correo { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Usuario")]
    public string UserName { get; set; } = string.Empty;
}
