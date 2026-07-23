using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealStateApp.Core.Application.DTOs.Propiedades;
using RealStateApp.Core.Application.Interfaces.Services;
using RealStateApp.WebApp.ViewModels.Propiedades;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

[Authorize(Roles = "Agente,Administrador")]
public class PropiedadController : Controller
{
    private readonly IPropiedadService _propiedadService;
    private readonly ITipoPropiedadService _tipoPropiedadService;
    private readonly ITipoVentaService _tipoVentaService;
    private readonly IMejoraService _mejoraService;
    private readonly IMapper _mapper;

    public PropiedadController(
        IPropiedadService propiedadService,
        ITipoPropiedadService tipoPropiedadService,
        ITipoVentaService tipoVentaService,
        IMejoraService mejoraService,
        IMapper mapper)
    {
        _propiedadService = propiedadService;
        _tipoPropiedadService = tipoPropiedadService;
        _tipoVentaService = tipoVentaService;
        _mejoraService = mejoraService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        string agenteId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        bool isAdmin = User.IsInRole("Administrador");

        var propiedades = isAdmin
            ? await _propiedadService.GetAllAsync()
            : await _propiedadService.GetPropiedadesByAgenteAsync(agenteId);

        return View(propiedades);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var propiedades = await _propiedadService.GetAllAsync();
        var propiedad = propiedades?.FirstOrDefault(p => p.Id == id);

        if (propiedad == null)
        {
            return NotFound();
        }

        return View(propiedad);
    }

    [HttpGet]
    public async Task<IActionResult> Crear()
    {
        var vm = new SavePropiedadViewModel
        {
            TiposPropiedades = await _tipoPropiedadService.GetAllDTOsAsync(),
            TiposVentas = await _tipoVentaService.GetAllDTOsAsync(),
            Mejoras = await _mejoraService.GetAllDTOsAsync()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(SavePropiedadViewModel vm)
    {
        if (vm.ArchivosImagenes == null || vm.ArchivosImagenes.Count == 0 || vm.ArchivosImagenes.Count > 4)
        {
            ModelState.AddModelError("ArchivosImagenes", "Debe seleccionar entre 1 y 4 imágenes.");
        }

        if (!ModelState.IsValid)
        {
            vm.TiposPropiedades = await _tipoPropiedadService.GetAllDTOsAsync();
            vm.TiposVentas = await _tipoVentaService.GetAllDTOsAsync();
            vm.Mejoras = await _mejoraService.GetAllDTOsAsync();
            return View(vm);
        }

        vm.AgenteId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var dto = _mapper.Map<SavePropiedadDTO>(vm);

        dto.ImagenesUrls = new List<string>();
        foreach (var file in vm.ArchivosImagenes)
        {
            if (file.Length > 0)
            {
                string basePath = $"/images/propiedades/{Guid.NewGuid()}_{file.FileName}";
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", basePath.TrimStart('/'));

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                dto.ImagenesUrls.Add(basePath);
            }
        }

        await _propiedadService.AddAsync(dto);

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var propiedades = await _propiedadService.GetAllAsync();
        var propiedadDto = propiedades?.FirstOrDefault(p => p.Id == id);

        if (propiedadDto == null)
        {
            return NotFound();
        }

        var vm = _mapper.Map<SavePropiedadViewModel>(propiedadDto);
        vm.TiposPropiedades = await _tipoPropiedadService.GetAllDTOsAsync();
        vm.TiposVentas = await _tipoVentaService.GetAllDTOsAsync();
        vm.Mejoras = await _mejoraService.GetAllDTOsAsync();

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(SavePropiedadViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.TiposPropiedades = await _tipoPropiedadService.GetAllDTOsAsync();
            vm.TiposVentas = await _tipoVentaService.GetAllDTOsAsync();
            vm.Mejoras = await _mejoraService.GetAllDTOsAsync();
            return View(vm);
        }

        string agenteId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var dto = _mapper.Map<SavePropiedadDTO>(vm);

        if (vm.ArchivosImagenes != null && vm.ArchivosImagenes.Count > 0)
        {
            dto.ImagenesUrls = new List<string>();
            foreach (var file in vm.ArchivosImagenes)
            {
                if (file.Length > 0)
                {
                    string basePath = $"/images/propiedades/{Guid.NewGuid()}_{file.FileName}";
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", basePath.TrimStart('/'));

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    dto.ImagenesUrls.Add(basePath);
                }
            }
        }
        else
        {
            dto.ImagenesUrls = null;
        }

        await _propiedadService.UpdateAsync(dto, vm.Id, agenteId);

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        string agenteId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _propiedadService.DeleteAsync(id, agenteId);
        return RedirectToAction("Index");
    }
}