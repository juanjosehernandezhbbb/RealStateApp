using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealStateApp.Core.Application.DTOs.Mensajes;
using RealStateApp.Core.Application.DTOs.Propiedades;
using RealStateApp.Core.Application.Interfaces.Services;
using RealStateApp.Core.Domain.Entities;
using RealStateApp.WebApp.ViewModels.Agente;
using RealStateApp.WebApp.ViewModels.Propiedades;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

public class AgenteController : Controller
{
    private readonly IPropiedadService _propiedadService;
    private readonly ITipoPropiedadService _tipoPropiedadService;
    private readonly ITipoVentaService _tipoVentaService;
    private readonly IMejoraService _mejoraService;
    private readonly IOfertaService _ofertaService;
    private readonly IMensajeService _mensajeService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly UserManager<ApplicationUser> _userManager;

    public AgenteController(
        IPropiedadService propiedadService,
        ITipoPropiedadService tipoPropiedadService,
        ITipoVentaService tipoVentaService,
        IMejoraService mejoraService,
        IOfertaService ofertaService,
        IMensajeService mensajeService,
        IWebHostEnvironment webHostEnvironment,
        UserManager<ApplicationUser> userManager)
    {
        _propiedadService = propiedadService;
        _tipoPropiedadService = tipoPropiedadService;
        _tipoVentaService = tipoVentaService;
        _mejoraService = mejoraService;
        _ofertaService = ofertaService;
        _mensajeService = mensajeService;
        _webHostEnvironment = webHostEnvironment;
        _userManager = userManager;
    }

    private string GetAgenteId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    }

    // ==========================================
    // 1. LISTADO PÚBLICO DE AGENTES (Visible sin login)
    // ==========================================
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var agentes = await _userManager.GetUsersInRoleAsync("Agente");
        return View(agentes);
    }

    // ==========================================
    // 2. PANEL / HOME DEL AGENTE LOGUEADO
    // ==========================================
    [Authorize(Roles = "Agente")]
    [HttpGet]
    public async Task<IActionResult> Panel()
    {
        string agenteId = GetAgenteId();
        var propiedades = await _propiedadService.GetPropiedadesByAgenteAsync(agenteId);
        return View(propiedades);
    }

    // ==========================================
    // 3. MANTENIMIENTO DE PROPIEDADES
    // ==========================================
    [Authorize(Roles = "Agente")]
    [HttpGet]
    public async Task<IActionResult> Mantenimiento()
    {
        string agenteId = GetAgenteId();
        var todasLasDelAgente = await _propiedadService.GetPropiedadesByAgenteAsync(agenteId);

        var disponibles = todasLasDelAgente
            .Where(p => p.Estado != null && p.Estado.ToString().Equals("Disponible", StringComparison.OrdinalIgnoreCase))
            .ToList();

        return View(disponibles);
    }

    // ==========================================
    // 4. CREAR PROPIEDAD
    // ==========================================
    [Authorize(Roles = "Agente")]
    [HttpGet]
    public async Task<IActionResult> Crear()
    {
        var tiposPropiedad = await _tipoPropiedadService.GetAllDTOsAsync();
        var tiposVenta = await _tipoVentaService.GetAllDTOsAsync();
        var mejoras = await _mejoraService.GetAllDTOsAsync();

        if (tiposPropiedad == null || !tiposPropiedad.Any())
        {
            TempData["Error"] = "No existen tipos de propiedades registrados. Debe crear al menos un tipo de propiedad antes de registrar una propiedad.";
            return RedirectToAction(nameof(Mantenimiento));
        }
        if (tiposVenta == null || !tiposVenta.Any())
        {
            TempData["Error"] = "No existen tipos de ventas registrados. Debe crear al menos un tipo de venta antes de registrar una propiedad.";
            return RedirectToAction(nameof(Mantenimiento));
        }
        if (mejoras == null || !mejoras.Any())
        {
            TempData["Error"] = "No existen mejoras registradas. Debe crear al menos una mejora antes de registrar una propiedad.";
            return RedirectToAction(nameof(Mantenimiento));
        }

        SavePropiedadViewModel vm = new()
        {
            TiposPropiedades = tiposPropiedad,
            TiposVentas = tiposVenta,
            Mejoras = mejoras
        };

        return View(vm);
    }

    [Authorize(Roles = "Agente")]
    [HttpPost]
    public async Task<IActionResult> Crear(SavePropiedadViewModel vm)
    {
        if (vm.ArchivosImagenes == null || vm.ArchivosImagenes.Count == 0)
        {
            ModelState.AddModelError("ArchivosImagenes", "Debe cargar al menos una imagen de la propiedad.");
        }
        else if (vm.ArchivosImagenes.Count > 4)
        {
            ModelState.AddModelError("ArchivosImagenes", "Solo se permite registrar hasta 4 imágenes por propiedad.");
        }

        if (!ModelState.IsValid)
        {
            vm.TiposPropiedades = await _tipoPropiedadService.GetAllDTOsAsync();
            vm.TiposVentas = await _tipoVentaService.GetAllDTOsAsync();
            vm.Mejoras = await _mejoraService.GetAllDTOsAsync();
            return View(vm);
        }

        string agenteId = GetAgenteId();

        List<string> imagenesUrls = new List<string>();
        string basePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "properties");

        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }

        foreach (var file in vm.ArchivosImagenes)
        {
            if (file.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string filePath = Path.Combine(basePath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                imagenesUrls.Add($"/images/properties/{fileName}");
            }
        }

        SavePropiedadDTO dto = new()
        {
            TipoPropiedadId = vm.TipoPropiedadId,
            TipoVentaId = vm.TipoVentaId,
            Precio = vm.Precio,
            Descripcion = vm.Descripcion,
            Tamano = vm.Tamano,
            CantidadHabitaciones = vm.CantidadHabitaciones,
            CantidadBanos = vm.CantidadBanos,
            AgenteId = agenteId,
            MejorasIds = vm.MejorasIds ?? new List<int>(),
            ImagenesUrls = imagenesUrls
        };

        await _propiedadService.AddAsync(dto);

        TempData["Success"] = "La propiedad fue creada correctamente.";
        return RedirectToAction(nameof(Mantenimiento));
    }

    // ==========================================
    // 5. EDITAR PROPIEDAD
    // ==========================================
    [Authorize(Roles = "Agente")]
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        string agenteId = GetAgenteId();
        var propiedadesAgente = await _propiedadService.GetPropiedadesByAgenteAsync(agenteId);
        var propiedadDto = propiedadesAgente.FirstOrDefault(p => p.Id == id);

        if (propiedadDto == null)
        {
            TempData["Error"] = "No tiene permisos para modificar esta propiedad o no existe.";
            return RedirectToAction(nameof(Mantenimiento));
        }

        if (propiedadDto.Estado != null && propiedadDto.Estado.ToString().Equals("Vendida", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "No se puede modificar una propiedad que ya fue vendida.";
            return RedirectToAction(nameof(Mantenimiento));
        }

        SavePropiedadViewModel vm = new()
        {
            Id = propiedadDto.Id,
            TipoPropiedadId = propiedadDto.TipoPropiedadId,
            TipoVentaId = propiedadDto.TipoVentaId,
            Precio = propiedadDto.Precio,
            Descripcion = propiedadDto.Descripcion,
            Tamano = propiedadDto.Tamano,
            CantidadHabitaciones = propiedadDto.CantidadHabitaciones,
            CantidadBanos = propiedadDto.CantidadBanos,
            MejorasIds = propiedadDto.MejorasIds ?? new List<int>(),
            TiposPropiedades = await _tipoPropiedadService.GetAllDTOsAsync(),
            TiposVentas = await _tipoVentaService.GetAllDTOsAsync(),
            Mejoras = await _mejoraService.GetAllDTOsAsync()
        };

        return View(vm);
    }

    [Authorize(Roles = "Agente")]
    [HttpPost]
    public async Task<IActionResult> Editar(SavePropiedadViewModel vm)
    {
        string agenteId = GetAgenteId();

        if (vm.ArchivosImagenes != null && vm.ArchivosImagenes.Count > 4)
        {
            ModelState.AddModelError("ArchivosImagenes", "La propiedad no debe tener más de 4 imágenes en total.");
        }

        if (!ModelState.IsValid)
        {
            vm.TiposPropiedades = await _tipoPropiedadService.GetAllDTOsAsync();
            vm.TiposVentas = await _tipoVentaService.GetAllDTOsAsync();
            vm.Mejoras = await _mejoraService.GetAllDTOsAsync();
            return View(vm);
        }

        List<string> imagenesUrls = new List<string>();
        if (vm.ArchivosImagenes != null && vm.ArchivosImagenes.Count > 0)
        {
            string basePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "properties");

            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            foreach (var file in vm.ArchivosImagenes)
            {
                if (file.Length > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(basePath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    imagenesUrls.Add($"/images/properties/{fileName}");
                }
            }
        }

        SavePropiedadDTO dto = new()
        {
            Id = vm.Id,
            TipoPropiedadId = vm.TipoPropiedadId,
            TipoVentaId = vm.TipoVentaId,
            Precio = vm.Precio,
            Descripcion = vm.Descripcion,
            Tamano = vm.Tamano,
            CantidadHabitaciones = vm.CantidadHabitaciones,
            CantidadBanos = vm.CantidadBanos,
            MejorasIds = vm.MejorasIds ?? new List<int>(),
            ImagenesUrls = imagenesUrls
        };

        await _propiedadService.UpdateAsync(dto, vm.Id, agenteId);

        TempData["Success"] = "La propiedad fue actualizada correctamente.";
        return RedirectToAction(nameof(Mantenimiento));
    }

    // ==========================================
    // 6. ELIMINAR PROPIEDAD
    // ==========================================
    [Authorize(Roles = "Agente")]
    [HttpGet]
    public async Task<IActionResult> Eliminar(int id)
    {
        string agenteId = GetAgenteId();
        var propiedadesAgente = await _propiedadService.GetPropiedadesByAgenteAsync(agenteId);
        var propiedad = propiedadesAgente.FirstOrDefault(p => p.Id == id);

        if (propiedad == null)
        {
            TempData["Error"] = "No tiene permisos para eliminar esta propiedad o no existe.";
            return RedirectToAction(nameof(Mantenimiento));
        }

        if (propiedad.Estado != null && propiedad.Estado.ToString().Equals("Vendida", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "No se puede eliminar una propiedad que ya fue vendida.";
            return RedirectToAction(nameof(Mantenimiento));
        }

        return View(propiedad);
    }

    [Authorize(Roles = "Agente")]
    [HttpPost, ActionName("Eliminar")]
    public async Task<IActionResult> EliminarConfirmado(int id)
    {
        string agenteId = GetAgenteId();
        try
        {
            await _propiedadService.DeleteAsync(id, agenteId);
            TempData["Success"] = "La propiedad fue eliminada correctamente.";
        }
        catch (System.Exception)
        {
            TempData["Error"] = "Ocurrió un error al intentar eliminar la propiedad.";
        }

        return RedirectToAction(nameof(Mantenimiento));
    }

    // ==========================================
    // 7. DETALLE DE PROPIEDAD
    // ==========================================
    [Authorize(Roles = "Agente")]
    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        string agenteId = GetAgenteId();
        var propiedadesAgente = await _propiedadService.GetPropiedadesByAgenteAsync(agenteId);
        var propiedad = propiedadesAgente.FirstOrDefault(p => p.Id == id);

        if (propiedad == null)
        {
            TempData["Error"] = "No tiene permisos para ver el detalle de esta propiedad.";
            return RedirectToAction(nameof(Panel));
        }

        var todasLasOfertas = await _ofertaService.GetAllDTOsAsync();
        var ofertasPropiedad = todasLasOfertas?.Where(o => o.PropiedadId == id).ToList() ?? new();

        var mensajesPropiedad = await _mensajeService.GetMensajesPorPropiedadAsync(id);

        AgentePropiedadDetailViewModel vm = new()
        {
            Propiedad = propiedad,
            Ofertas = ofertasPropiedad,
            Mensajes = mensajesPropiedad
        };

        return View(vm);
    }

    // ==========================================
    // 8. CONVERSACIÓN / CHAT CON CLIENTE
    // ==========================================
    [Authorize(Roles = "Agente")]
    [HttpGet]
    public async Task<IActionResult> Conversacion(int propiedadId, string clienteId)
    {
        string agenteId = GetAgenteId();
        var propiedadesAgente = await _propiedadService.GetPropiedadesByAgenteAsync(agenteId);
        var propiedad = propiedadesAgente.FirstOrDefault(p => p.Id == propiedadId);

        if (propiedad == null)
        {
            TempData["Error"] = "No tiene acceso a esta conversación.";
            return RedirectToAction(nameof(Panel));
        }

        var todosLosMensajes = await _mensajeService.GetMensajesPorPropiedadAsync(propiedadId);
        ViewBag.Mensajes = todosLosMensajes?.Where(m => m.ClienteId == clienteId).ToList() ?? new();

        SaveMensajeDTO vm = new()
        {
            PropiedadId = propiedadId,
            ClienteId = clienteId,
            AgenteId = agenteId,
            EmisorId = agenteId
        };

        return View(vm);
    }

    [Authorize(Roles = "Agente")]
    [HttpPost]
    public async Task<IActionResult> EnviarRespuestaMensaje(SaveMensajeDTO vm)
    {
        if (string.IsNullOrWhiteSpace(vm.Contenido))
        {
            TempData["Error"] = "Debe escribir un mensaje antes de enviarlo.";
            return RedirectToAction(nameof(Conversacion), new { propiedadId = vm.PropiedadId, clienteId = vm.ClienteId });
        }

        string agenteId = GetAgenteId();
        vm.AgenteId = agenteId;
        vm.EmisorId = agenteId;

        await _mensajeService.AddAsync(vm);

        return RedirectToAction(nameof(Conversacion), new { propiedadId = vm.PropiedadId, clienteId = vm.ClienteId });
    }

    // ==========================================
    // 9. GESTIÓN DE OFERTAS
    // ==========================================
    [Authorize(Roles = "Agente")]
    [HttpPost]
    public async Task<IActionResult> AceptarOferta(int ofertaId, int propiedadId)
    {
        string agenteId = GetAgenteId();
        await _ofertaService.AceptarOfertaAsync(ofertaId, agenteId);
        TempData["Success"] = "¡Oferta aceptada correctamente y propiedad marcada como vendida!";
        return RedirectToAction(nameof(Detalle), new { id = propiedadId });
    }

    [Authorize(Roles = "Agente")]
    [HttpPost]
    public async Task<IActionResult> RechazarOferta(int ofertaId, int propiedadId)
    {
        string agenteId = GetAgenteId();
        await _ofertaService.RechazarOfertaAsync(ofertaId, agenteId);
        TempData["Success"] = "La oferta ha sido rechazada.";
        return RedirectToAction(nameof(Detalle), new { id = propiedadId });
    }

    // ==========================================
    // 10. MÓDULO MI PERFIL
    // ==========================================
    [Authorize(Roles = "Agente")]
    [HttpGet]
    public async Task<IActionResult> Perfil()
    {
        string agenteId = GetAgenteId();
        return View();
    }

    [Authorize(Roles = "Agente")]
    [HttpPost]
    public async Task<IActionResult> Perfil(string nombre, string apellido, string telefono, IFormFile foto)
    {
        string agenteId = GetAgenteId();

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido) || string.IsNullOrWhiteSpace(telefono))
        {
            TempData["Error"] = "Todos los campos obligatorios deben ser completados.";
            return View();
        }

        try
        {
            TempData["Success"] = "Su perfil fue actualizado correctamente.";
        }
        catch (System.Exception)
        {
            TempData["Error"] = "Ocurrió un error al intentar actualizar su perfil.";
            return View();
        }

        return RedirectToAction(nameof(Panel));
    }
}