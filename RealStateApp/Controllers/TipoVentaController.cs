using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealStateApp.ViewModels.TipoVentas;
using RealStateApp.Core.Domain.Entities;
using RealStateApp.Persistance.Contexts;

namespace RealStateApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class TipoVentaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TipoVentaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /TipoVenta/Index
        public async Task<IActionResult> Index()
        {
            var tiposVenta = await _context.TipoVentas
                .Include(tv => tv.Propiedades)
                .ToListAsync();

            var vmList = tiposVenta.Select(tv => new TipoVentaViewModel
            {
                Id = tv.Id,
                Nombre = tv.Nombre,
                Descripcion = tv.Descripcion,
                CantidadPropiedades = tv.Propiedades != null ? tv.Propiedades.Count : 0
            }).ToList();

            return View(vmList);
        }

        // GET: /TipoVenta/Crear
        public IActionResult Crear()
        {
            return View(new SaveTipoVentaViewModel());
        }

        // POST: /TipoVenta/Crear
        [HttpPost]
        public async Task<IActionResult> Crear(SaveTipoVentaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Debe completar todos los campos requeridos.";
                return View(model);
            }

            model.Nombre = model.Nombre.Trim();
            model.Descripcion = model.Descripcion.Trim();

            // Validar nombre único
            var existe = await _context.TipoVentas.AnyAsync(tv => tv.Nombre.ToLower() == model.Nombre.ToLower());
            if (existe)
            {
                ModelState.AddModelError("Nombre", "Ya existe un tipo de venta registrado con este nombre.");
                TempData["Error"] = "Ya existe un tipo de venta registrado con este nombre.";
                return View(model);
            }

            var tipoVenta = new TipoVenta
            {
                Nombre = model.Nombre,
                Descripcion = model.Descripcion
            };

            _context.TipoVentas.Add(tipoVenta);
            await _context.SaveChangesAsync();

            TempData["Success"] = "El tipo de venta fue creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /TipoVenta/Editar/{id}
        public async Task<IActionResult> Editar(int id)
        {
            var tipoVenta = await _context.TipoVentas.FindAsync(id);
            if (tipoVenta == null)
            {
                TempData["Error"] = "El tipo de venta seleccionado no existe.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new SaveTipoVentaViewModel
            {
                Id = tipoVenta.Id,
                Nombre = tipoVenta.Nombre,
                Descripcion = tipoVenta.Descripcion
            };

            return View(vm);
        }

        // POST: /TipoVenta/Editar
        [HttpPost]
        public async Task<IActionResult> Editar(SaveTipoVentaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Debe completar todos los campos requeridos.";
                return View(model);
            }

            model.Nombre = model.Nombre.Trim();
            model.Descripcion = model.Descripcion.Trim();

            var tipoVenta = await _context.TipoVentas.FindAsync(model.Id);
            if (tipoVenta == null)
            {
                TempData["Error"] = "El tipo de venta seleccionado no existe.";
                return RedirectToAction(nameof(Index));
            }

            // Validar nombre único excluyendo el actual
            var existe = await _context.TipoVentas.AnyAsync(tv => tv.Nombre.ToLower() == model.Nombre.ToLower() && tv.Id != model.Id);
            if (existe)
            {
                ModelState.AddModelError("Nombre", "Ya existe otro tipo de venta registrado con este nombre.");
                TempData["Error"] = "Ya existe otro tipo de venta registrado con este nombre.";
                return View(model);
            }

            tipoVenta.Nombre = model.Nombre;
            tipoVenta.Descripcion = model.Descripcion;

            _context.TipoVentas.Update(tipoVenta);
            await _context.SaveChangesAsync();

            TempData["Success"] = "El tipo de venta fue actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /TipoVenta/Eliminar
        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var tipoVenta = await _context.TipoVentas
                    .Include(tv => tv.Propiedades)
                        .ThenInclude(p => p.Imagenes) // <- Cambiado a 'Imagenes'
                    .Include(tv => tv.Propiedades)
                        .ThenInclude(p => p.PropiedadMejoras)
                    .Include(tv => tv.Propiedades)
                        .ThenInclude(p => p.Favoritos)
                    .Include(tv => tv.Propiedades)
                        .ThenInclude(p => p.Ofertas)
                    .Include(tv => tv.Propiedades)
                        .ThenInclude(p => p.Mensajes)
                    .FirstOrDefaultAsync(tv => tv.Id == id);

                if (tipoVenta == null)
                {
                    TempData["Error"] = "El tipo de venta seleccionado no existe.";
                    return RedirectToAction(nameof(Index));
                }

                if (tipoVenta.Propiedades != null && tipoVenta.Propiedades.Any())
                {
                    foreach (var prop in tipoVenta.Propiedades)
                    {
                        if (prop.Imagenes != null && prop.Imagenes.Any()) // <- Cambiado a 'prop.Imagenes'
                        {
                            _context.PropiedadImagenes.RemoveRange(prop.Imagenes); // El DbSet del contexto sí se llama PropiedadImagenes
                        }

                        if (prop.PropiedadMejoras != null && prop.PropiedadMejoras.Any())
                        {
                            _context.PropiedadMejoras.RemoveRange(prop.PropiedadMejoras);
                        }

                        if (prop.Favoritos != null && prop.Favoritos.Any())
                        {
                            _context.Favoritos.RemoveRange(prop.Favoritos);
                        }

                        if (prop.Ofertas != null && prop.Ofertas.Any())
                        {
                            _context.Ofertas.RemoveRange(prop.Ofertas);
                        }

                        if (prop.Mensajes != null && prop.Mensajes.Any())
                        {
                            _context.Mensajes.RemoveRange(prop.Mensajes);
                        }
                    }

                    _context.Propiedades.RemoveRange(tipoVenta.Propiedades);
                }

                _context.TipoVentas.Remove(tipoVenta);
                await _context.SaveChangesAsync();

                TempData["Success"] = "El tipo de venta fue eliminado correctamente.";
            }
            catch (Exception)
            {
                TempData["Error"] = "No fue posible eliminar el tipo de venta. Intente nuevamente más tarde.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}