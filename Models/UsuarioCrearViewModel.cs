using System.ComponentModel.DataAnnotations;

namespace Inventarios.Models;

public class UsuarioCrearViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Correo")]
    public string Correo { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Contrasena { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar Contraseña")]
    [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmarContrasena { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Rol")]
    public string Rol { get; set; } = "Usuario";
}
