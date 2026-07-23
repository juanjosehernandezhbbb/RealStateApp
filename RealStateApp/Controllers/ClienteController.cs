using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RealStateApp.Core.Application.DTOs.Favoritos;
using RealStateApp.Core.Application.DTOs.Mensajes;
using RealStateApp.Core.Application.DTOs.Ofertas;
using RealStateApp.Core.Application.DTOs.Usuarios;
using RealStateApp.Core.Application.Interfaces.Services;
using RealStateApp.Core.Application.Services;
using RealStateApp.Core.Application.ViewModels.Cliente;
using RealStateApp.Core.Domain.Entities;
using System.Security.Claims;

namespace RealStateApp.WebApp.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class ClienteController : Controller
    {
        private readonly IPropiedadService _propiedadService;
        private readonly IFavoritoService _favoritoService;
        private readonly ITipoPropiedadService _tipoPropiedadService;
        private readonly IOfertaService _ofertaService;
        private readonly IMensajeService _mensajeService;
        private readonly IAccountService _accountService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClienteController(
            IPropiedadService propiedadService,
            IFavoritoService favoritoService,
            ITipoPropiedadService tipoPropiedadService,
            IOfertaService ofertaService,
            IMensajeService mensajeService,
            IAccountService accountService,
            UserManager<ApplicationUser> userManager)
        {
            _propiedadService = propiedadService;
            _favoritoService = favoritoService;
            _tipoPropiedadService = tipoPropiedadService;
            _ofertaService = ofertaService;
            _mensajeService = mensajeService;
            _accountService = accountService;
            _userManager = userManager;
        }

        // 1. HOME DEL CLIENTE CON FILTROS
        public async Task<IActionResult> Index(string? codigoFiltro, int? tipoPropiedadId, decimal? precioMin, decimal? precioMax, int? habitaciones, int? banos)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var propiedades = await _propiedadService.GetAllDTOsAsync();

            // Filtrar solo propiedades disponibles y ordenadas por más recientes
            propiedades = propiedades.Where(p => p.Estado == "Disponible").OrderByDescending(p => p.Id).ToList();

            // Aplicar Filtros de búsqueda
            if (!string.IsNullOrEmpty(codigoFiltro))
            {
                propiedades = propiedades.Where(p => p.Codigo.Contains(codigoFiltro, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (tipoPropiedadId.HasValue && tipoPropiedadId.Value > 0)
            {
                propiedades = propiedades.Where(p => p.TipoPropiedadId == tipoPropiedadId.Value).ToList();
            }

            if (precioMin.HasValue)
            {
                propiedades = propiedades.Where(p => p.Precio >= precioMin.Value).ToList();
            }

            if (precioMax.HasValue)
            {
                propiedades = propiedades.Where(p => p.Precio <= precioMax.Value).ToList();
            }

            if (habitaciones.HasValue)
            {
                propiedades = propiedades.Where(p => p.CantidadHabitaciones == habitaciones.Value).ToList();
            }

            if (banos.HasValue)
            {
                propiedades = propiedades.Where(p => p.CantidadBanos == banos.Value).ToList();
            }

            var favoritos = await _favoritoService.GetAllDTOsAsync();
            var idsFavoritosUsuario = favoritos
                .Where(f => f.UsuarioId == userId)
                .Select(f => f.PropiedadFavoritaId)
                .ToHashSet();

            foreach (var propiedad in propiedades)
            {
                propiedad.EsFavorito = idsFavoritosUsuario.Contains(propiedad.Id);
            }

            ViewBag.CodigoFiltro = codigoFiltro;
            ViewBag.PrecioMin = precioMin;
            ViewBag.PrecioMax = precioMax;
            ViewBag.Habitaciones = habitaciones;
            ViewBag.Banos = banos;

            var tiposPropiedad = await _tipoPropiedadService.GetAllDTOsAsync();
            ViewBag.TiposPropiedad = new SelectList(tiposPropiedad, "Id", "Nombre", tipoPropiedadId);

            return View(propiedades);
        }

        // 2. MIS PROPIEDADES (Favoritas y disponibles)
        public async Task<IActionResult> MisPropiedades()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var favoritos = await _favoritoService.GetAllDTOsAsync();

            var propiedadesFavoritas = favoritos
                .Where(f => f.UsuarioId == userId && f.Propiedad != null && f.Propiedad.Estado == "Disponible")
                .Select(f => f.Propiedad!)
                .ToList();

            foreach (var prop in propiedadesFavoritas)
            {
                prop.EsFavorito = true;
            }

            return View(propiedadesFavoritas);
        }

        // 3. TOGGLE FAVORITO (Marcar/Desmarcar)
        [HttpPost]
        public async Task<IActionResult> ToggleFavorito(int propiedadId, string returnUrl = "")
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var favoritos = await _favoritoService.GetAllDTOsAsync();
            var favoritoExistente = favoritos.FirstOrDefault(f => f.UsuarioId == userId && f.PropiedadFavoritaId == propiedadId);

            if (favoritoExistente != null)
            {
                await _favoritoService.DeleteAsync(favoritoExistente.Id);
            }
            else
            {
                SaveFavoritoDTO saveDto = new()
                {
                    UsuarioId = userId,
                    PropiedadFavoritaId = propiedadId
                };
                await _favoritoService.AddAsync(saveDto);
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Obtener la propiedad
            var propiedades = await _propiedadService.GetAllDTOsAsync();
            var propiedad = propiedades.FirstOrDefault(p => p.Id == id);

            if (propiedad == null)
            {
                return NotFound();
            }

            // Obtener ofertas
            var todasLasOfertas = await _ofertaService.GetAllDTOsAsync();

            // Filtrar ofertas realizadas por este cliente (CompradorId)
            var ofertasCliente = todasLasOfertas
                .Where(o => o.PropiedadId == id && o.CompradorId == userId)
                .ToList();

            // Reglas de validación para los botones y alertas
            bool tieneOfertaPendiente = ofertasCliente.Any(o => o.Estado == "Pendiente");
            bool tieneOfertaAceptadaGlobal = todasLasOfertas.Any(o => o.PropiedadId == id && o.Estado == "Aceptada");

            // Obtener mensajes del chat asociados a esta propiedad y cliente (ClienteId)
            var todosLosMensajes = await _mensajeService.GetAllDTOsAsync();
            var mensajesPropiedad = todosLosMensajes
                .Where(m => m.PropiedadId == id && m.ClienteId == userId)
                .ToList();

            ClientePropiedadDetailViewModel vm = new()
            {
                Propiedad = propiedad,
                Ofertas = ofertasCliente,
                Mensajes = mensajesPropiedad,
                TieneOfertaPendiente = tieneOfertaPendiente,
                TieneOfertaAceptadaGlobal = tieneOfertaAceptadaGlobal
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> EnviarMensaje(int propiedadId, string mensajeTexto)
        {
            if (string.IsNullOrWhiteSpace(mensajeTexto))
            {
                TempData["Error"] = "Debe escribir un mensaje antes de enviarlo.";
                return RedirectToAction(nameof(Details), new { id = propiedadId });
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Obtenemos la propiedad para extraer el ID del agente dueño
            var propiedades = await _propiedadService.GetAllDTOsAsync();
            var propiedad = propiedades.FirstOrDefault(p => p.Id == propiedadId);

            if (propiedad == null)
            {
                return NotFound();
            }

            SaveMensajeDTO saveDto = new()
            {
                PropiedadId = propiedadId,
                ClienteId = userId,
                AgenteId = propiedad.AgenteId,
                EmisorId = userId,
                Contenido = mensajeTexto
            };

            await _mensajeService.AddAsync(saveDto);

            TempData["Success"] = "Mensaje enviado correctamente.";
            return RedirectToAction(nameof(Details), new { id = propiedadId });
        }

        [HttpPost]
        public async Task<IActionResult> EnviarOferta(int propiedadId, decimal montoOferta)
        {
            if (montoOferta <= 0)
            {
                TempData["Error"] = "El monto de la oferta debe ser un valor numérico mayor que cero.";
                return RedirectToAction(nameof(Details), new { id = propiedadId });
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            SaveOfertaDTO saveDto = new()
            {
                PropiedadId = propiedadId,
                CompradorId = userId,
                Monto = montoOferta
            };

            await _ofertaService.AddAsync(saveDto);

            TempData["Success"] = "La oferta fue enviada correctamente.";
            return RedirectToAction(nameof(Details), new { id = propiedadId });
        }

        // ==========================================
        // LISTADO DE AGENTES (Menú del Cliente)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Agentes(string nombreFiltro)
        {
            var todosLosAgentes = await _userManager.GetUsersInRoleAsync("Agente");

            // FILTRAR ESTRICTAMENTE LOS ACTIVOS AQUÍ TAMBIÉN
            var agentesActivos = todosLosAgentes
                .Where(a => a.EstaActivo)
                .AsQueryable();

            if (!string.IsNullOrEmpty(nombreFiltro))
            {
                agentesActivos = agentesActivos.Where(a =>
                    (!string.IsNullOrEmpty(a.Nombre) && a.Nombre.Contains(nombreFiltro, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(a.Apellido) && a.Apellido.Contains(nombreFiltro, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(a.UserName) && a.UserName.Contains(nombreFiltro, StringComparison.OrdinalIgnoreCase))
                );
            }

            var agentesDTO = agentesActivos
                .OrderBy(a => a.Nombre)
                .ThenBy(a => a.Apellido)
                .Select(a => new UsuarioDTO
                {
                    Id = a.Id,
                    Nombre = a.Nombre,
                    Apellido = a.Apellido,
                    Email = a.Email,
                    Telefono = !string.IsNullOrEmpty(a.Telefono) ? a.Telefono : (!string.IsNullOrEmpty(a.PhoneNumber) ? a.PhoneNumber : "No registrado"),
                    FotoPerfilUrl = a.FotoPerfilUrl,
                    EstaActivo = a.EstaActivo
                }).ToList();

            ViewBag.NombreFiltro = nombreFiltro;
            return View(agentesDTO);
        }
    }
}