using Inventarios.Data;
using Inventarios.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Inventarios.Controllers;

[Authorize]
public class PrestamosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public PrestamosController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var query = _context.Prestamos
            .Include(p => p.Usuario)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.HerramientaUnidad!)
                    .ThenInclude(u => u.Herramienta)
            .AsQueryable();

        if (!User.IsInRole("Admin"))
        {
            var usuarioId = _userManager.GetUserId(User) ?? string.Empty;
            query = query.Where(p => p.UsuarioId == usuarioId);
        }

        var prestamos = await query.AsNoTracking().ToListAsync();
        return View(prestamos);
    }

    public async Task<IActionResult> Crear(string? estante = null, Guid? herramientaId = null)
    {
        var viewModel = await ConstruirViewModelAsync(estante, herramientaId);
        if (!viewModel.HerramientasDisponibles.Any())
        {
            TempData["Error"] = "No hay unidades disponibles para prestar.";
        }
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(PrestamoCrearViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            modelo = await ConstruirViewModelAsync(modelo.Estante, modelo.HerramientaId);
            return View(modelo);
        }

        var unidades = await _context.HerramientasUnidades
            .Include(u => u.Herramienta)
            .Where(u => u.HerramientaId == modelo.HerramientaId && u.Estado == EstadoHerramientaUnidad.Disponible)
            .Take(modelo.Cantidad)
            .ToListAsync();

        if (unidades.Count < modelo.Cantidad)
        {
            TempData["Error"] = "La unidad seleccionada no está disponible.";
            ModelState.AddModelError(string.Empty, "La unidad seleccionada no está disponible.");
            modelo = await ConstruirViewModelAsync(modelo.Estante);
            return View(modelo);
        }

        var usuarioId = _userManager.GetUserId(User) ?? string.Empty;

        var prestamo = new Prestamo
        {
            UsuarioId = usuarioId,
            Estado = EstadoPrestamo.Activo,
            FechaPrestamo = DateTime.UtcNow,
            Detalles = new List<PrestamoDetalle>()
        };

        foreach (var unidad in unidades)
        {
            prestamo.Detalles.Add(new PrestamoDetalle
            {
                Prestamo = prestamo,
                HerramientaUnidad = unidad
            });
            unidad.Estado = EstadoHerramientaUnidad.Prestada;
            if (unidad.Herramienta != null && unidad.Herramienta.CantidadDisponible > 0)
            {
                unidad.Herramienta.CantidadDisponible -= 1;
            }
        }

        _context.Prestamos.Add(prestamo);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SolicitarDevolucion(Guid id)
    {
        var prestamo = await _context.Prestamos
            .Include(p => p.Detalles)
                .ThenInclude(d => d.HerramientaUnidad)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (prestamo == null) return NotFound();

        var usuarioId = _userManager.GetUserId(User) ?? string.Empty;
        if (prestamo.UsuarioId != usuarioId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        if (prestamo.Estado == EstadoPrestamo.Activo)
        {
            prestamo.Estado = EstadoPrestamo.DevueltoPendiente;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ConfirmarDevolucion(Guid id)
    {
        var prestamo = await _context.Prestamos
            .Include(p => p.Usuario)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.HerramientaUnidad!)
                    .ThenInclude(u => u.Herramienta)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (prestamo == null) return NotFound();

        return View(prestamo);
    }
    
    public async Task<IActionResult> Detalle(Guid id)
    {
        var prestamo = await _context.Prestamos
            .Include(p => p.Usuario)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.HerramientaUnidad!)
                    .ThenInclude(u => u.Herramienta)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
        if (prestamo == null) return NotFound();
        return View(prestamo);
    }

    [HttpPost, ActionName("ConfirmarDevolucion")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarDevolucionPost(Guid id, string estadoHerramienta, string? observaciones)
    {
        var prestamo = await _context.Prestamos
            .Include(p => p.Usuario)
            .Include(p => p.Detalles)
                .ThenInclude(d => d.HerramientaUnidad!)
                    .ThenInclude(u => u.Herramienta)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (prestamo == null) return NotFound();

        prestamo.Estado = EstadoPrestamo.Cerrado;
        prestamo.FechaDevolucion = DateTime.UtcNow;

        foreach (var detalle in prestamo.Detalles)
        {
            if (detalle.HerramientaUnidad != null)
            {
                // Mapear estado devuelto
                bool dejarDisponible = estadoHerramienta == "ok" || estadoHerramienta == "leve";
                detalle.HerramientaUnidad.Estado = dejarDisponible ? EstadoHerramientaUnidad.Disponible : EstadoHerramientaUnidad.Danada;
                detalle.HerramientaUnidad.Observacion = observaciones;

                if (dejarDisponible && detalle.HerramientaUnidad.Herramienta != null)
                {
                    detalle.HerramientaUnidad.Herramienta.CantidadDisponible += 1;
                }
            }
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task<PrestamoCrearViewModel> ConstruirViewModelAsync(string? estanteSeleccionado = null, Guid? herramientaSeleccionada = null)
    {
        var estantes = new[] { "Estante A", "Estante B", "Estante C", "Estante D" };
        var estante = estanteSeleccionado ?? estantes.First();

        var herramientas = await _context.Herramientas
            .Include(h => h.Unidades)
            .Where(h => h.Estante == estante && h.Unidades.Any(u => u.Estado == EstadoHerramientaUnidad.Disponible))
            .AsNoTracking()
            .ToListAsync();

        var items = herramientas.Select(h => new SelectListItem
        {
            Value = h.Id.ToString(),
            Text = $"{h.Codigo} - {h.Nombre} ({h.Estante}) | Disp: {h.Unidades.Count(u => u.Estado == EstadoHerramientaUnidad.Disponible)}",
            Selected = herramientaSeleccionada.HasValue && h.Id == herramientaSeleccionada.Value
        }).ToList();

        var herramienta = herramientas.FirstOrDefault(h => h.Id == herramientaSeleccionada) ?? herramientas.FirstOrDefault();
        var disponibles = herramienta?.Unidades.Count(u => u.Estado == EstadoHerramientaUnidad.Disponible) ?? 0;

        return new PrestamoCrearViewModel
        {
            Estante = estante,
            Estantes = estantes.Select(e => new SelectListItem { Value = e, Text = e, Selected = e == estante }).ToList(),
            HerramientasDisponibles = items,
            HerramientaId = herramienta?.Id ?? Guid.Empty,
            DisponiblesHerramienta = disponibles
        };
    }
}
