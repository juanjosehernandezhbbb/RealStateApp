using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealStateApp.Core.Application.DTOs.Account; 
using RealStateApp.Core.Application.DTOs.Usuarios;
using RealStateApp.Core.Application.DTOs.Propiedades;
using RealStateApp.Core.Application.Interfaces.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador,Desarrollador")]
    public class AgentController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IPropiedadService _propiedadService;

        public AgentController(IAccountService accountService, IPropiedadService propiedadService)
        {
            _accountService = accountService;
            _propiedadService = propiedadService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<UsuarioDTO>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            var usuarios = await _accountService.GetAllUsersAsync();
            var agentes = usuarios.Where(u => u.Roles != null && u.Roles.Contains("Agente")).ToList();

            if (agentes == null || !agentes.Any())
                return NoContent();

            return Ok(agentes);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UsuarioDTO))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(string id)
        {
            var agente = await _accountService.GetUserByIdAsync(id);

            if (agente == null || agente.Roles == null || !agente.Roles.Contains("Agente"))
                return NotFound(new { message = "El agente no existe." });

            return Ok(agente);
        }

        [HttpGet("{id}/Property")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PropiedadDTO>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)] 
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPropertiesByAgent(string id)
        {
            var agente = await _accountService.GetUserByIdAsync(id);
            if (agente == null || agente.Roles == null || !agente.Roles.Contains("Agente"))
                return NotFound(new { message = "El agente no existe." });

            var propiedades = await _propiedadService.GetPropiedadesByAgenteAsync(id);

            if (!propiedades.Any())
                return NoContent();

            return Ok(propiedades);
        }

        [HttpPatch("change-status/{id}")] 
        [Authorize(Roles = "Administrador")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangeStatus(string id, [FromBody] ChangeStatusDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var currentAdminId = User.FindFirst("uid")?.Value;

            await _accountService.ChangeUserStatusAsync(id, request.EstaActivo, currentAdminId);

            return Ok(new { message = $"El estado del agente ha sido actualizado a: {(request.EstaActivo ? "Activo" : "Inactivo")}." });
        }
    }
}