using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealStateApp.Core.Application.Interfaces.Services;
using System.Linq;
using System.Threading.Tasks;

[AllowAnonymous]
public class PropiedadController : Controller
{
    private readonly IPropiedadService _propiedadService;

    public PropiedadController(IPropiedadService propiedadService)
    {
        _propiedadService = propiedadService;
    }

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
}