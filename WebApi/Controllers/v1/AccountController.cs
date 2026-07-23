using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealStateApp.Core.Application.DTOs.Account;
using RealStateApp.Core.Application.DTOs.Usuarios;
using RealStateApp.Core.Application.Interfaces.Services;
using System.Threading.Tasks;

namespace WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> AuthenticateAsync([FromBody] AuthenticationRequest request)
        {
            var response = await _accountService.AuthenticateAsync(request);

            if (response.HasError)
            {
                return Unauthorized(new { message = response.Error });
            }

            if (response.Roles != null && (response.Roles.Contains("Cliente") || response.Roles.Contains("Agente")))
            {
                return StatusCode(403, new { message = "Los usuarios con rol Cliente o Agente no pueden autenticarse en la API." });
            }

            return Ok(response);
        }

        [HttpPost("register")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterApiRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre) ||
                string.IsNullOrWhiteSpace(request.Apellido) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Cedula) ||
                string.IsNullOrWhiteSpace(request.Telefono) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Todos los campos son requeridos (Nombre, Apellido, Email, Username, Cedula, Telefono, Password, ConfirmPassword, Rol)." });
            }

            if (request.Password != request.ConfirmPassword)
            {
                return BadRequest(new { message = "La contraseña y la confirmación de contraseña no coinciden." });
            }

            if (request.Rol != "Administrador" && request.Rol != "Desarrollador")
            {
                return BadRequest(new { message = "Desde la API solo se permite registrar usuarios con rol de Administrador o Desarrollador." });
            }

            var saveUsuarioDto = new SaveUsuarioDTO
            {
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Email = request.Email,
                UserName = request.Username,
                Cedula = request.Cedula,
                Telefono = request.Telefono,
                Password = request.Password,
                Rol = request.Rol
            };

            var origin = Request.Headers["origin"];
            var response = await _accountService.RegisterUserAsync(saveUsuarioDto, origin);

            if (response != "Success")
            {
                return BadRequest(new { message = response });
            }

            return Ok(new { message = $"Usuario con rol {request.Rol} registrado exitosamente." });
        }
    }
}