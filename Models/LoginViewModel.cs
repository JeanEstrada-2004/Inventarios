using System.ComponentModel.DataAnnotations;

namespace Inventarios.Models;

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Correo")]
    public string Correo { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Contrasena { get; set; } = string.Empty;
}
