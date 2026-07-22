using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealStateApp.Core.Application.DTOs.Propiedades;
using RealStateApp.Core.Application.Interfaces.Services;
using RealStateApp.WebApp.ViewModels.Propiedades;
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
        if (!ModelState.IsValid)
        {
            vm.TiposPropiedades = await _tipoPropiedadService.GetAllDTOsAsync();
            vm.TiposVentas = await _tipoVentaService.GetAllDTOsAsync();
            vm.Mejoras = await _mejoraService.GetAllDTOsAsync();
            return View(vm);
        }

        vm.AgenteId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var dto = _mapper.Map<SavePropiedadDTO>(vm);
        await _propiedadService.AddAsync(dto);

        return RedirectToAction("Index");
    }
}