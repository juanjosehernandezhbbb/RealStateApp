using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealStateApp.Core.Application.DTOs.Mejoras;
using RealStateApp.Core.Application.Interfaces.Services;
using Asp.Versioning;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador,Desarrollador")]
    public class MejoraController : ControllerBase
    {
        private readonly IMejoraService _mejoraService;

        public MejoraController(IMejoraService mejoraService)
        {
            _mejoraService = mejoraService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<MejoraDTO>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            var mejoras = await _mejoraService.GetAllDTOsAsync();

            if (mejoras == null || !mejoras.Any())
                return NoContent();

            return Ok(mejoras);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MejoraDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(int id)
        {
            var mejoras = await _mejoraService.GetAllDTOsAsync();
            var mejora = mejoras.FirstOrDefault(m => m.Id == id);

            if (mejora == null) return NotFound(new { message = "La mejora no existe." });

            return Ok(mejora);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] SaveMejoraDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Descripcion))
            {
                return BadRequest(new { message = "El nombre y la descripción son requeridos." });
            }

            var result = await _mejoraService.AddAsync(dto);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Put(int id, [FromBody] SaveMejoraDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Descripcion))
            {
                return BadRequest(new { message = "El nombre y la descripción son requeridos." });
            }

            var existing = await _mejoraService.GetByIdSaveDTOAsync(id);
            if (existing == null) return NotFound(new { message = "La mejora no existe." });

            dto.Id = id;
            await _mejoraService.UpdateAsync(dto, id);

            return Ok(dto);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _mejoraService.GetByIdSaveDTOAsync(id);
            if (existing == null) return NotFound(new { message = "La mejora no existe." });

            await _mejoraService.DeleteAsync(id);
            return NoContent();
        }
    }
}