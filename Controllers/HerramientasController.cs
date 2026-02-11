using Inventarios.Data;
using Inventarios.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventarios.Controllers;

[Authorize]
public class HerramientasController : Controller
{
    private readonly ApplicationDbContext _context;

    public HerramientasController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Listado para todos los usuarios autenticados
    public async Task<IActionResult> Index()
    {
        var herramientas = await _context.Herramientas.AsNoTracking().ToListAsync();
        return View(herramientas);
    }

    public async Task<IActionResult> Detalle(Guid id)
    {
        var herramienta = await _context.Herramientas
            .Include(h => h.Unidades)
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == id);

        if (herramienta == null) return NotFound();
        return View(herramienta);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Crear()
    {
        return View(new Herramienta());
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(Herramienta herramienta)
    {
        if (!ModelState.IsValid)
        {
            return View(herramienta);
        }

        herramienta.CantidadDisponible = herramienta.CantidadTotal;
        _context.Herramientas.Add(herramienta);

        // Se crean unidades individuales para controlar disponibilidad
        for (int i = 0; i < herramienta.CantidadTotal; i++)
        {
            _context.HerramientasUnidades.Add(new HerramientaUnidad
            {
                Herramienta = herramienta,
                Estado = EstadoHerramientaUnidad.Disponible,
                Etiqueta = $"Unidad {i + 1}"
            });
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Editar(Guid id)
    {
        var herramienta = await _context.Herramientas.FindAsync(id);
        if (herramienta == null) return NotFound();
        return View(herramienta);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Guid id, Herramienta modelo)
    {
        if (id != modelo.Id) return NotFound();
        if (!ModelState.IsValid) return View(modelo);

        var herramienta = await _context.Herramientas
            .Include(h => h.Unidades)
            .FirstOrDefaultAsync(h => h.Id == id);
        if (herramienta == null) return NotFound();

        herramienta.Codigo = modelo.Codigo;
        herramienta.Nombre = modelo.Nombre;
        herramienta.Marca = modelo.Marca;
        herramienta.Estante = modelo.Estante;
        // Ajuste unidades
        var diferencia = modelo.CantidadTotal - herramienta.CantidadTotal;
        if (diferencia > 0)
        {
            var start = herramienta.Unidades.Count;
            for (int i = 0; i < diferencia; i++)
            {
                var nueva = new HerramientaUnidad
                {
                    HerramientaId = herramienta.Id,
                    Estado = EstadoHerramientaUnidad.Disponible,
                    Etiqueta = $"Unidad {start + i + 1}"
                };
                _context.HerramientasUnidades.Add(nueva);
                herramienta.Unidades.Add(nueva);
            }
        }
        else if (diferencia < 0)
        {
            var aEliminar = -diferencia;
            var disponibles = herramienta.Unidades
                .Where(u => u.Estado == EstadoHerramientaUnidad.Disponible)
                .Take(aEliminar)
                .ToList();
            if (disponibles.Count < aEliminar)
            {
                ModelState.AddModelError(string.Empty, "No puedes reducir la cantidad porque hay unidades prestadas o dañadas.");
                return View(modelo);
            }
            _context.HerramientasUnidades.RemoveRange(disponibles);
            foreach (var u in disponibles)
            {
                herramienta.Unidades.Remove(u);
            }
        }

        herramienta.CantidadTotal = modelo.CantidadTotal;
        herramienta.Especificacion = modelo.Especificacion;
        herramienta.Observaciones = modelo.Observaciones;
        // recalcular disponibles
        herramienta.CantidadDisponible = herramienta.Unidades.Count(u => u.Estado == EstadoHerramientaUnidad.Disponible);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        var herramienta = await _context.Herramientas.FindAsync(id);
        if (herramienta == null) return NotFound();
        return View(herramienta);
    }

    [HttpPost, ActionName("Eliminar")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarConfirmado(Guid id)
    {
        var herramienta = await _context.Herramientas
            .Include(h => h.Unidades)
            .FirstOrDefaultAsync(h => h.Id == id);
        if (herramienta == null) return NotFound();

        _context.HerramientasUnidades.RemoveRange(herramienta.Unidades);
        _context.Herramientas.Remove(herramienta);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> EditarUnidad(Guid id)
    {
        var unidad = await _context.HerramientasUnidades
            .Include(u => u.Herramienta)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (unidad == null) return NotFound();
        return View(unidad);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarUnidad(Guid id, HerramientaUnidad modelo)
    {
        if (id != modelo.Id) return NotFound();
        var unidad = await _context.HerramientasUnidades
            .Include(u => u.Herramienta!)
                .ThenInclude(h => h.Unidades)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (unidad == null) return NotFound();

        unidad.Etiqueta = modelo.Etiqueta;
        unidad.Estado = modelo.Estado;
        unidad.Observacion = modelo.Observacion;

        // Recalcular disponibilidad de la herramienta asociada
        if (unidad.Herramienta != null)
        {
            unidad.Herramienta.CantidadDisponible = unidad.Herramienta.Unidades.Count(u => u.Estado == EstadoHerramientaUnidad.Disponible);
        }

        await _context.SaveChangesAsync();
        TempData["Mensaje"] = "Unidad actualizada.";
        return RedirectToAction(nameof(Detalle), new { id = unidad.HerramientaId });
    }
}
