using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealStateApp.ViewModels.Mejoras;
using RealStateApp.Core.Domain.Entities;
using RealStateApp.Persistance.Contexts;

namespace RealStateApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class MejoraController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MejoraController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Mejora/Index
        public async Task<IActionResult> Index()
        {
            var mejoras = await _context.Mejoras
                .Include(m => m.PropiedadMejoras)
                .ToListAsync();

            var vmList = mejoras.Select(m => new MejoraViewModel
            {
                Id = m.Id,
                Nombre = m.Nombre,
                Descripcion = m.Descripcion,
                CantidadPropiedades = m.PropiedadMejoras != null ? m.PropiedadMejoras.Count : 0
            }).ToList();

            return View(vmList);
        }

        // GET: /Mejora/Crear
        public IActionResult Crear()
        {
            return View(new SaveMejoraViewModel());
        }

        // POST: /Mejora/Crear
        [HttpPost]
        public async Task<IActionResult> Crear(SaveMejoraViewModel model)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.Nombre) || string.IsNullOrWhiteSpace(model.Descripcion))
            {
                TempData["Error"] = "Debe completar todos los campos requeridos.";
                return View(model);
            }

            model.Nombre = model.Nombre.Trim();
            model.Descripcion = model.Descripcion.Trim();

            var existe = await _context.Mejoras.AnyAsync(m => m.Nombre.ToLower() == model.Nombre.ToLower());
            if (existe)
            {
                ModelState.AddModelError("Nombre", "Ya existe una mejora registrada con este nombre.");
                TempData["Error"] = "Ya existe una mejora registrada con este nombre.";
                return View(model);
            }

            var mejora = new Mejora
            {
                Nombre = model.Nombre,
                Descripcion = model.Descripcion
            };

            _context.Mejoras.Add(mejora);
            await _context.SaveChangesAsync();

            TempData["Success"] = "La mejora fue creada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Mejora/Editar/{id}
        public async Task<IActionResult> Editar(int id)
        {
            var mejora = await _context.Mejoras.FindAsync(id);
            if (mejora == null)
            {
                TempData["Error"] = "La mejora seleccionada no existe.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new SaveMejoraViewModel
            {
                Id = mejora.Id,
                Nombre = mejora.Nombre,
                Descripcion = mejora.Descripcion
            };

            return View(vm);
        }

        // POST: /Mejora/Editar
        [HttpPost]
        public async Task<IActionResult> Editar(SaveMejoraViewModel model)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.Nombre) || string.IsNullOrWhiteSpace(model.Descripcion))
            {
                TempData["Error"] = "Debe completar todos los campos requeridos.";
                return View(model);
            }

            model.Nombre = model.Nombre.Trim();
            model.Descripcion = model.Descripcion.Trim();

            var mejora = await _context.Mejoras.FindAsync(model.Id);
            if (mejora == null)
            {
                TempData["Error"] = "La mejora seleccionada no existe.";
                return RedirectToAction(nameof(Index));
            }

            var existe = await _context.Mejoras.AnyAsync(m => m.Nombre.ToLower() == model.Nombre.ToLower() && m.Id != model.Id);
            if (existe)
            {
                ModelState.AddModelError("Nombre", "Ya existe otra mejora registrada con este nombre.");
                TempData["Error"] = "Ya existe otra mejora registrada con este nombre.";
                return View(model);
            }

            mejora.Nombre = model.Nombre;
            mejora.Descripcion = model.Descripcion;

            _context.Mejoras.Update(mejora);
            await _context.SaveChangesAsync();

            TempData["Success"] = "La mejora fue actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Mejora/Eliminar
        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var mejora = await _context.Mejoras
                    .Include(m => m.PropiedadMejoras)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (mejora == null)
                {
                    TempData["Error"] = "La mejora seleccionada no existe.";
                    return RedirectToAction(nameof(Index));
                }

                if (mejora.PropiedadMejoras != null && mejora.PropiedadMejoras.Any())
                {
                    _context.PropiedadMejoras.RemoveRange(mejora.PropiedadMejoras);
                }

                _context.Mejoras.Remove(mejora);
                await _context.SaveChangesAsync();

                TempData["Success"] = "La mejora fue eliminada correctamente.";
            }
            catch (Exception)
            {
                TempData["Error"] = "No fue posible eliminar la mejora. Intente nuevamente más tarde.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}