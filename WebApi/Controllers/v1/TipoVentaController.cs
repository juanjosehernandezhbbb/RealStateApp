using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealStateApp.Core.Application.DTOs.TipoVentas;
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
    public class TipoVentaController : ControllerBase
    {
        private readonly ITipoVentaService _tipoVentaService;

        public TipoVentaController(ITipoVentaService tipoVentaService)
        {
            _tipoVentaService = tipoVentaService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<TipoVentaDTO>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            var tipos = await _tipoVentaService.GetAllDTOsAsync();

            if (tipos == null || !tipos.Any())
                return NoContent();

            return Ok(tipos);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TipoVentaDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(int id)
        {
            var tipos = await _tipoVentaService.GetAllDTOsAsync();
            var tipo = tipos.FirstOrDefault(t => t.Id == id);

            if (tipo == null) return NotFound(new { message = "El tipo de venta no existe." });

            return Ok(tipo);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody] SaveTipoVentaDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Descripcion))
            {
                return BadRequest(new { message = "El nombre y la descripción son requeridos." });
            }

            var result = await _tipoVentaService.AddAsync(dto);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Put(int id, [FromBody] SaveTipoVentaDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Descripcion))
            {
                return BadRequest(new { message = "El nombre y la descripción son requeridos." });
            }

            var existing = await _tipoVentaService.GetByIdSaveDTOAsync(id);
            if (existing == null) return NotFound(new { message = "El tipo de venta no existe." });

            dto.Id = id;
            await _tipoVentaService.UpdateAsync(dto, id);

            return Ok(dto);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _tipoVentaService.GetByIdSaveDTOAsync(id);
            if (existing == null) return NotFound(new { message = "El tipo de venta no existe." });

            await _tipoVentaService.DeleteAsync(id);
            return NoContent();
        }
    }
}