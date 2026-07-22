using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealStateApp.Core.Application.DTOs.Account;
using RealStateApp.Core.Application.DTOs.Usuarios;
using RealStateApp.Core.Application.Interfaces.Services;
using RealStateApp.WebApp.ViewModels.Account;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RealStateApp.WebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IWebHostEnvironment _env;

        public AccountController(IAccountService accountService, IWebHostEnvironment env)
        {
            _accountService = accountService;
            _env = env;
        }

        // ==========================================
        // FLUJO DE LOGIN
        // ==========================================
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var request = new AuthenticationRequest
            {
                EmailOrUserName = vm.EmailOrUserName,
                Password = vm.Password
            };

            var response = await _accountService.AuthenticateAsync(request);

            if (response.HasError)
            {
                ModelState.AddModelError(string.Empty, response.Error ?? "Error al iniciar sesión.");
                return View(vm);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, response.Id),
                new Claim(ClaimTypes.Name, response.UserName),
                new Claim(ClaimTypes.Email, response.Email)
            };

            foreach (var rol in response.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, rol));
            }

            var claimsIdentity = new ClaimsIdentity(claims, "Identity.Application");
            await HttpContext.SignInAsync("Identity.Application", new ClaimsPrincipal(claimsIdentity));

            if (response.Roles.Contains("Administrador"))
                return RedirectToAction("Index", "Admin");

            if (response.Roles.Contains("Agente"))
                return RedirectToAction("Index", "Agente");

            return RedirectToAction("Index", "Cliente");
        }

        // ==========================================
        // FLUJO DE REGISTRO (Únete a la App)
        // ==========================================
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new SaveUsuarioDTO());
        }

        [HttpPost]
        public async Task<IActionResult> Register(SaveUsuarioDTO model)
        {
            // Validar la foto de perfil
            if (model.File == null || model.File.Length == 0)
            {
                ModelState.AddModelError("File", "Debe subir una foto de perfil.");
            }
            else
            {
                var extension = Path.GetExtension(model.File.FileName).ToLowerInvariant();
                if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                {
                    ModelState.AddModelError("File", "El archivo seleccionado no tiene un formato de imagen válido.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Guardar imagen usando tu IWebHostEnvironment
            model.FotoPerfilUrl = await UploadImageAsync(model.File);

            // Origin para el link del correo
            string origin = $"{Request.Scheme}://{Request.Host}";

            string result = await _accountService.RegisterUserAsync(model, origin);

            if (result == "Success")
            {
                if (model.Rol == "Cliente")
                {
                    TempData["MensajeExito"] = "Su cuenta ha sido creada correctamente. Revise su correo electrónico para activar su usuario.";
                }
                else if (model.Rol == "Agente")
                {
                    TempData["MensajeExito"] = "Su cuenta de agente ha sido creada correctamente. Un administrador debe activar su usuario antes de que pueda iniciar sesión.";
                }

                return RedirectToAction("Login");
            }

            // Mapeo de errores desde Identity hacia su respectivo campo
            if (result.Contains("correo", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("Email", result);
            }
            else if (result.Contains("nombre de usuario", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("UserName", result);
            }
            else if (result.Contains("contraseña", StringComparison.OrdinalIgnoreCase) || result.Contains("password", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("Password", result);
            }
            else
            {
                ModelState.AddModelError(string.Empty, result);
            }

            return View(model);
        }

        // ==========================================
        // ACTIVACIÓN DE CORREO
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            string response = await _accountService.ConfirmEmailAsync(userId, token);
            if (response == "Success")
            {
                TempData["MensajeExito"] = "Su cuenta ha sido activada correctamente. Ya puede iniciar sesión.";
            }
            else
            {
                TempData["MensajeError"] = response;
            }

            return RedirectToAction("Login");
        }

        // ==========================================
        // LOGOUT
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("Login");
        }

        // ==========================================
        // HELPERS
        // ==========================================
        private async Task<string> UploadImageAsync(IFormFile file)
        {
            // Usamos tu _env.WebRootPath para que sea 100% compatible con ASP.NET Core
            string folderPath = Path.Combine(_env.WebRootPath, "Images", "Users");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            string filePath = Path.Combine(folderPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/Images/Users/{uniqueFileName}";
        }

        // ==========================================
        // REENVIAR CORREO DE ACTIVACIÓN
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> ResendVerificationEmail(string emailOrUserName)
        {
            if (string.IsNullOrWhiteSpace(emailOrUserName))
            {
                TempData["MensajeError"] = "Por favor, ingrese su correo o nombre de usuario para reenviar el correo.";
                return RedirectToAction("Login");
            }

            string origin = $"{Request.Scheme}://{Request.Host}";
            string result = await _accountService.SendVerificationEmailAsync(emailOrUserName, origin);

            if (result == "Success")
            {
                TempData["MensajeExito"] = "Se ha enviado un nuevo enlace de activación a su correo electrónico.";
            }
            else
            {
                TempData["MensajeError"] = result;
            }

            return RedirectToAction("Login");
        }
    }
}