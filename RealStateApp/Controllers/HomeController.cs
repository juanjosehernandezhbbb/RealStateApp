using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RealStateApp.Core.Application.Interfaces.Services;
using System.Threading.Tasks;
using System;
using System.Linq;

public class HomeController : Controller
{
    private readonly IPropiedadService _propiedadService;
    private readonly ITipoPropiedadService _tipoPropiedadService;

    public HomeController(IPropiedadService propiedadService, ITipoPropiedadService tipoPropiedadService)
    {
        _propiedadService = propiedadService;
        _tipoPropiedadService = tipoPropiedadService;
    }

    public async Task<IActionResult> Index(string codigoFiltro, int? tipoPropiedadId, decimal? precioMin, decimal? precioMax, int? habitaciones, int? banos)
    {
        // ==========================================
        // REDIRECCIÓN SEGÚN EL ROL SI ESTÁ AUTENTICADO
        // ==========================================
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            if (User.IsInRole("Administrador"))
            {
                return RedirectToAction("Index", "Admin");
            }
            else if (User.IsInRole("Agente"))
            {
                return RedirectToAction("Index", "Agente");
            }
            else if (User.IsInRole("Cliente"))
            {
                return RedirectToAction("Index", "Cliente");
            }
        }

        // ==========================================
        // HOME PÚBLICO (Usuarios no autenticados)
        // ==========================================

        // 1. Obtenemos todas las propiedades
        var todasLasPropiedades = await _propiedadService.GetAllDTOsAsync();

        // 2. Filtramos de forma segura comparando el texto del estado
        var propiedades = todasLasPropiedades
            .Where(p => p.Estado != null && p.Estado.ToString().Equals("Disponible", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // 3. Búsqueda por código
        if (!string.IsNullOrEmpty(codigoFiltro))
        {
            propiedades = propiedades.Where(p => p.Codigo != null && p.Codigo.Contains(codigoFiltro, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        else
        {
            // 4. Aplicar filtros combinados
            if (tipoPropiedadId.HasValue && tipoPropiedadId.Value > 0)
            {
                propiedades = propiedades.Where(p => p.TipoPropiedadId == tipoPropiedadId.Value).ToList();
            }
            if (precioMin.HasValue && precioMin.Value >= 0)
            {
                propiedades = propiedades.Where(p => p.Precio >= precioMin.Value).ToList();
            }
            if (precioMax.HasValue && precioMax.Value >= 0)
            {
                propiedades = propiedades.Where(p => p.Precio <= precioMax.Value).ToList();
            }
            if (habitaciones.HasValue && habitaciones.Value >= 0)
            {
                propiedades = propiedades.Where(p => p.CantidadHabitaciones == habitaciones.Value).ToList();
            }
            if (banos.HasValue && banos.Value >= 0)
            {
                propiedades = propiedades.Where(p => p.CantidadBanos == banos.Value).ToList();
            }
        }

        // Cargar los tipos de propiedad para el select del formulario
        var listaTipos = await _tipoPropiedadService.GetAllDTOsAsync();
        ViewBag.TiposPropiedad = new SelectList(listaTipos, "Id", "Nombre", tipoPropiedadId);

        // Mantener los valores actuales en la vista
        ViewBag.CodigoFiltro = codigoFiltro;
        ViewBag.PrecioMin = precioMin;
        ViewBag.PrecioMax = precioMax;
        ViewBag.Habitaciones = habitaciones;
        ViewBag.Banos = banos;

        return View(propiedades);
    }
}