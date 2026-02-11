namespace Inventarios.Models;

public class UsuarioRolViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}
