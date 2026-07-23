using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using RealStateApp.Core.Application.Interfaces.Services;
using RealStateApp.Core.Application.DTOs.Propiedades;
using RealStateApp.Core.Application.DTOs.Usuarios;
using RealStateApp.Core.Application.ViewModels;
using RealStateApp.Core.Domain.Entities;
using System.Threading.Tasks;
using System;
using System.Linq;

public class HomeController : Controller
{
    private readonly IPropiedadService _propiedadService;
    private readonly ITipoPropiedadService _tipoPropiedadService;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(
        IPropiedadService propiedadService,
        ITipoPropiedadService tipoPropiedadService,
        UserManager<ApplicationUser> userManager)
    {
        _propiedadService = propiedadService;
        _tipoPropiedadService = tipoPropiedadService;
        _userManager = userManager;
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

        var todasLasPropiedades = await _propiedadService.GetAllDTOsAsync();

        var propiedades = todasLasPropiedades
            .Where(p => p.Estado != null && p.Estado.ToString().Equals("Disponible", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!string.IsNullOrEmpty(codigoFiltro))
        {
            propiedades = propiedades.Where(p => p.Codigo != null && p.Codigo.Contains(codigoFiltro, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        else
        {
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

        var listaTipos = await _tipoPropiedadService.GetAllDTOsAsync();
        ViewBag.TiposPropiedad = new SelectList(listaTipos, "Id", "Nombre", tipoPropiedadId);

        ViewBag.CodigoFiltro = codigoFiltro;
        ViewBag.PrecioMin = precioMin;
        ViewBag.PrecioMax = precioMax;
        ViewBag.Habitaciones = habitaciones;
        ViewBag.Banos = banos;

        return View(propiedades);
    }

    // ==========================================
    // MÓDULO PÚBLICO: LISTADO Y BÚSQUEDA DE AGENTES
    // ==========================================
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Agentes(string nombreFiltro)
    {
        var todosLosAgentes = await _userManager.GetUsersInRoleAsync("Agente");

        var agentesActivos = todosLosAgentes
            .Where(a => a.EstaActivo)
            .AsQueryable();

        if (!string.IsNullOrEmpty(nombreFiltro))
        {
            agentesActivos = agentesActivos.Where(a =>
                (!string.IsNullOrEmpty(a.Nombre) && a.Nombre.Contains(nombreFiltro, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(a.Apellido) && a.Apellido.Contains(nombreFiltro, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(a.UserName) && a.UserName.Contains(nombreFiltro, StringComparison.OrdinalIgnoreCase))
            );
        }

        var agentesDTO = agentesActivos
            .OrderBy(a => a.Nombre)
            .ThenBy(a => a.Apellido)
            .Select(a => new UsuarioDTO
            {
                Id = a.Id,
                Nombre = a.Nombre,
                Apellido = a.Apellido,
                Email = a.Email,
                // Validamos primero Telefono y como respaldo PhoneNumber
                Telefono = !string.IsNullOrEmpty(a.Telefono) ? a.Telefono : (!string.IsNullOrEmpty(a.PhoneNumber) ? a.PhoneNumber : "No registrado"),
                FotoPerfilUrl = a.FotoPerfilUrl,
                EstaActivo = a.EstaActivo
            }).ToList();

        ViewBag.NombreFiltro = nombreFiltro;
        return View(agentesDTO);
    }

    // ==========================================
    // MÓDULO PÚBLICO: PROPIEDADES DE UN AGENTE
    // ==========================================
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> PropiedadesPorAgente(string id)
    {
        var agente = await _userManager.FindByIdAsync(id);

        // Validar que el agente exista y esté activo
        if (agente == null || !agente.EstaActivo)
        {
            TempData["Error"] = "El agente solicitado no existe o no se encuentra disponible.";
            return RedirectToAction(nameof(Agentes));
        }

        var propiedadesAgente = await _propiedadService.GetPropiedadesByAgenteAsync(id);

        var disponibles = propiedadesAgente?
            .Where(p => p.Estado != null && p.Estado.ToString().Equals("Disponible", StringComparison.OrdinalIgnoreCase))
            .ToList() ?? new();

        var propiedadesViewModel = disponibles.Select(p => new PropertyViewModel
        {
            Id = p.Id,
            Codigo = p.Codigo,
            Descripcion = p.Descripcion,
            TipoPropiedadId = p.TipoPropiedadId,
            TipoPropiedad = p.TipoPropiedadNombre,
            TipoVentaId = p.TipoVentaId,
            TipoVenta = p.TipoVentaNombre,
            Precio = p.Precio,
            CantidadHabitaciones = p.CantidadHabitaciones,
            CantidadBanos = p.CantidadBanos,
            Tamano = (double)p.Tamano,
            ImagenPrincipalUrl = p.ImagenesUrls != null && p.ImagenesUrls.Any() ? p.ImagenesUrls.First() : string.Empty,
            AgenteId = p.AgenteId,
            Estado = p.Estado?.ToString()
        }).ToList();

        ViewBag.Agente = agente;
        return View(propiedadesViewModel);
    }
}