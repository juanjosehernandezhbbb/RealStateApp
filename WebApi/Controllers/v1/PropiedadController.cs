using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealStateApp.Core.Application.DTOs.Propiedades;
using RealStateApp.Core.Application.Interfaces.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize] 
    public class PropiedadController : ControllerBase
    {
        private readonly IPropiedadService _propiedadService;

        public PropiedadController(IPropiedadService propiedadService)
        {
            _propiedadService = propiedadService;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Desarrollador")] 
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PropiedadDTO>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            var propiedades = await _propiedadService.GetAllDTOsAsync();

            if (propiedades == null || !propiedades.Any())
                return NoContent(); 

            return Ok(propiedades);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PropiedadDTO))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(int id)
        {
            var propiedades = await _propiedadService.GetAllDTOsAsync();
            var propiedad = propiedades.FirstOrDefault(p => p.Id == id);

            if (propiedad == null)
                return NotFound(new { message = "La propiedad no existe." });

            return Ok(propiedad);
        }

        [HttpGet("codigo/{codigo}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PropiedadDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByCodigo(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo) || !Regex.IsMatch(codigo, @"^\d{6}$"))
                return BadRequest(new { message = "El código debe tener exactamente 6 dígitos." });

            var propiedad = await _propiedadService.GetByCodeAsync(codigo);

            if (propiedad == null)
                return NotFound(new { message = "No se encontró ninguna propiedad con este código." });

            return Ok(propiedad);
        }
    }
}