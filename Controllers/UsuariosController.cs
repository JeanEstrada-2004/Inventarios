using Inventarios.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventarios.Controllers;

[Authorize]
public class UsuariosController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public UsuariosController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index()
    {
        var usuarios = new List<UsuarioRolViewModel>();
        var users = await _userManager.Users.AsNoTracking().ToListAsync();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            usuarios.Add(new UsuarioRolViewModel
            {
                Id = user.Id,
                Correo = user.Email ?? user.UserName ?? string.Empty,
                Roles = roles.ToList()
            });
        }

        return View(usuarios);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Crear()
    {
        ViewBag.Roles = new[] { "Admin", "Usuario" };
        return View(new UsuarioCrearViewModel());
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(UsuarioCrearViewModel modelo)
    {
        ViewBag.Roles = new[] { "Admin", "Usuario" };

        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        await AsegurarRolesAsync();

        var usuario = new IdentityUser
        {
            UserName = modelo.Correo,
            Email = modelo.Correo,
            EmailConfirmed = true
        };

        var resultado = await _userManager.CreateAsync(usuario, modelo.Contrasena);
        if (!resultado.Succeeded)
        {
            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(modelo);
        }

        await _userManager.AddToRoleAsync(usuario, modelo.Rol);

        TempData["Mensaje"] = "Usuario creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // Detalle para Admin
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Detalle(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();

        var user = await _userManager.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        var modelo = new UsuarioRolViewModel
        {
            Id = user.Id,
            Correo = user.Email ?? user.UserName ?? string.Empty,
            Roles = roles.ToList()
        };

        ViewBag.Roles = new[] { "Admin", "Usuario" };
        return View(modelo);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarRol(string id, string rol)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        if (rol != "Admin" && rol != "Usuario")
        {
            TempData["Error"] = "Rol inválido.";
            return RedirectToAction(nameof(Detalle), new { id });
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        await AsegurarRolesAsync();

        var rolesActuales = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, rolesActuales);
        await _userManager.AddToRoleAsync(user, rol);

        TempData["Mensaje"] = "Rol actualizado.";
        return RedirectToAction(nameof(Detalle), new { id });
    }

    // Perfil del usuario autenticado
    public async Task<IActionResult> Perfil()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var roles = await _userManager.GetRolesAsync(user);
        var modelo = new PerfilViewModel
        {
            Id = user.Id,
            Correo = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            Rol = roles.FirstOrDefault() ?? "Usuario"
        };
        return View(modelo);
    }

    public async Task<IActionResult> EditarPerfil()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var modelo = new PerfilEditarViewModel
        {
            Id = user.Id,
            Correo = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty
        };
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuardarPerfil(PerfilEditarViewModel modelo)
    {
        if (!ModelState.IsValid) return View("EditarPerfil", modelo);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        if (user.Id != modelo.Id)
        {
            TempData["Error"] = "No puedes editar otro perfil.";
            return RedirectToAction(nameof(Perfil));
        }

        user.Email = modelo.Correo;
        user.UserName = modelo.UserName;
        user.NormalizedEmail = modelo.Correo.ToUpperInvariant();
        user.NormalizedUserName = modelo.UserName.ToUpperInvariant();

        var resultado = await _userManager.UpdateAsync(user);
        if (!resultado.Succeeded)
        {
            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View("EditarPerfil", modelo);
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["Mensaje"] = "Perfil actualizado.";
        return RedirectToAction(nameof(Perfil));
    }

    // Confirmación de eliminación de usuario (Admin)
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Eliminar(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null) return NotFound();
        var roles = await _userManager.GetRolesAsync(usuario);
        return View(new UsuarioRolViewModel
        {
            Id = usuario.Id,
            Correo = usuario.Email ?? usuario.UserName ?? string.Empty,
            Roles = roles.ToList()
        });
    }

    [HttpPost, ActionName("Eliminar")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarConfirmado(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null) return NotFound();

        var resultado = await _userManager.DeleteAsync(usuario);
        if (!resultado.Succeeded)
        {
            TempData["Error"] = "No se pudo eliminar el usuario.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Mensaje"] = "Usuario eliminado.";
        return RedirectToAction(nameof(Index));
    }

    private async Task AsegurarRolesAsync()
    {
        var rolesNecesarios = new[] { "Admin", "Usuario" };
        foreach (var rol in rolesNecesarios)
        {
            if (!await _roleManager.RoleExistsAsync(rol))
            {
                await _roleManager.CreateAsync(new IdentityRole(rol));
            }
        }
    }
}
