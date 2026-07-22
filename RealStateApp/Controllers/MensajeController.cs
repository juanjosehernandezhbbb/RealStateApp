using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealStateApp.Core.Application.DTOs.Mensajes;
using RealStateApp.Core.Application.Interfaces.Services;
using System.Security.Claims;

namespace RealStateApp.WebApp.Controllers
{
    [Authorize]
    public class MensajeController : Controller
    {
        private readonly IMensajeService _mensajeService;

        public MensajeController(IMensajeService mensajeService)
        {
            _mensajeService = mensajeService;
        }

        // GET: /Mensaje/Index (Bandeja de entrada)
        public async Task<IActionResult> Index()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var mensajes = await _mensajeService.GetMensajesPorAgenteAsync(usuarioId);
            return View(mensajes);
        }

        // GET: /Mensaje/Conversacion
        public async Task<IActionResult> Conversacion(int propiedadId, string clienteId)
        {
            var historial = await _mensajeService.GetMensajesPorPropiedadAsync(propiedadId, clienteId);

            ViewBag.PropiedadId = propiedadId;
            ViewBag.ClienteId = clienteId;

            return View(historial);
        }

        // POST: /Mensaje/Enviar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enviar(SaveMensajeDTO dto, string? returnUrl = null)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            dto.EmisorId = usuarioId;
            await _mensajeService.AddAsync(dto);

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Conversacion), new { propiedadId = dto.PropiedadId, clienteId = dto.ClienteId });
        }
    }
}