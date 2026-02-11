using System.ComponentModel.DataAnnotations;

namespace Inventarios.Models;

public class CambiarContrasenaViewModel
{
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña actual")]
    public string ContrasenaActual { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva contraseña")]
    [MinLength(6)]
    public string ContrasenaNueva { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar nueva contraseña")]
    [Compare("ContrasenaNueva", ErrorMessage = "La confirmación no coincide.")]
    public string ConfirmarContrasena { get; set; } = string.Empty;
}
