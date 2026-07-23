using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RealStateApp.Core.Application.DTOs.Account;
using RealStateApp.Core.Application.DTOs.Usuarios;
using RealStateApp.Core.Application.Interfaces.Repositories;
using RealStateApp.Core.Application.Interfaces.Services;
using RealStateApp.Core.Application.Settings;
using RealStateApp.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealStateApp.Persistance.Identity.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly JWTSettings _jwtSettings;
        private readonly IPropiedadRepository _propiedadRepository;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            IOptions<JWTSettings> jwtSettings,
            IPropiedadRepository propiedadRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _jwtSettings = jwtSettings.Value;
            _propiedadRepository = propiedadRepository;
        }

        public async Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request)
        {
            var response = new AuthenticationResponse();
            var user = await _userManager.FindByEmailAsync(request.EmailOrUserName) ??
                       await _userManager.FindByNameAsync(request.EmailOrUserName);

            if (user == null)
            {
                response.HasError = true;
                response.Error = "Los datos de acceso son inválidos.";
                return response;
            }

            // 1. Validar si está activo ANTES de intentar el login o generar cookies
            if (!user.EstaActivo)
            {
                response.HasError = true;
                response.Error = "El usuario se encuentra inactivo y no puede iniciar sesión.";
                return response;
            }

            // 2. Intentar el inicio de sesión una vez confirmado que está activo
            var result = await _signInManager.PasswordSignInAsync(user.UserName, request.Password, false, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                response.HasError = true;

                if (result.IsNotAllowed)
                {
                    response.Error = "Identity bloqueó el acceso. (Revisa si en Program.cs se exige confirmar teléfono u otro dato).";
                }
                else if (result.IsLockedOut)
                {
                    response.Error = "La cuenta está bloqueada temporalmente.";
                }
                else
                {
                    response.Error = "La contraseña es incorrecta. Intenta registrar un usuario de prueba nuevo para asegurarte de la contraseña.";
                }

                return response;
            }

            var rolesList = await _userManager.GetRolesAsync(user);

            if (rolesList == null || rolesList.Count == 0)
            {
                response.HasError = true;
                response.Error = "El usuario no tiene un rol válido asignado. Póngase en contacto con un administrador.";
                return response;
            }

            response.Id = user.Id;
            response.Email = user.Email;
            response.UserName = user.UserName;
            response.Roles = rolesList.ToList();
            response.IsActive = user.EstaActivo;

            var tokenHandler = new Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler();
            var key = System.Text.Encoding.ASCII.GetBytes(_jwtSettings.Key);
            var claims = new List<System.Security.Claims.Claim>
    {
        new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, user.UserName),
        new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email, user.Email),
        new System.Security.Claims.Claim("uid", user.Id)
    };

            foreach (var role in rolesList) claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role));

            var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key), Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
            };

            response.Token = tokenHandler.CreateToken(tokenDescriptor);

            return response;
        }

        public async Task<string> RegisterUserAsync(SaveUsuarioDTO request, string origin)
        {
            // 1. Validar si el nombre de usuario ya existe
            var userByUserName = await _userManager.FindByNameAsync(request.UserName);
            if (userByUserName != null)
            {
                return "Ya existe un usuario registrado con este nombre de usuario.";
            }

            // 2. Validar si el correo electrónico ya existe
            var userByEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userByEmail != null)
            {
                return "Ya existe un usuario registrado con este correo electrónico.";
            }

            // 3. Validar cédula única (si fue ingresada)
            if (!string.IsNullOrWhiteSpace(request.Cedula) && await _userManager.Users.AnyAsync(u => u.Cedula == request.Cedula))
            {
                return "Ya existe un usuario registrado con esta cédula.";
            }

            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Cedula = request.Cedula,
                Telefono = request.Telefono,
                FotoPerfilUrl = request.FotoPerfilUrl,
                FechaRegistro = DateTime.Now,

                // Regla: Clientes y Agentes inician inactivos. Administradores o Desarrolladores inician activos.
                EstaActivo = request.Rol == "Administrador" || request.Rol == "Desarrollador"
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, request.Rol);

                // Flujo de activación por correo exclusivo para Clientes
                if (request.Rol == "Cliente")
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    var activationUrl = $"{origin}/Account/ConfirmEmail?userId={user.Id}&token={Uri.EscapeDataString(token)}";

                    string cuerpoCorreo = $@"
                        <h3>Hola {user.Nombre},</h3>
                        <p>Su cuenta ha sido registrada correctamente en RealEstateApp.</p>
                        <p>Para activar su usuario y poder iniciar sesión, utilice el siguiente enlace:</p>
                        <p><a href='{activationUrl}' style='padding: 10px 18px; background-color: #0d6efd; color: white; text-decoration: none; border-radius: 5px; font-weight: bold;'>Activar Cuenta</a></p>
                        <p>Si no puede hacer clic en el botón, copie y pegue la siguiente URL en su navegador:</p>
                        <p>{activationUrl}</p>
                        <br/>
                        <p><small>Si usted no realizó este registro, puede ignorar este mensaje.</small></p>";

                    await _emailService.SendAsync(
                        user.Email,
                        "Activación de cuenta en RealEstateApp",
                        cuerpoCorreo);
                }

                return "Success";
            }

            // Traduce y unifica los errores de Identity al español
            var erroresTraducidos = result.Errors.Select(e => e.Code switch
            {
                "PasswordTooShort" => "La contraseña es demasiado corta.",
                "PasswordRequiresNonAlphanumeric" => "La contraseña debe tener al menos un carácter no alfanumérico.",
                "PasswordRequiresDigit" => "La contraseña debe tener al menos un dígito ('0'-'9').",
                "PasswordRequiresUpper" => "La contraseña debe tener al menos una letra mayúscula ('A'-'Z').",
                "PasswordRequiresLower" => "La contraseña debe tener al menos una letra minúscula ('a'-'z').",
                _ => e.Description
            });

            return string.Join(" ", erroresTraducidos);
        }

        public async Task<string> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return "El usuario especificado no existe.";
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                user.EstaActivo = true;
                await _userManager.UpdateAsync(user);
                return "Success";
            }

            return "Ocurrió un error al activar la cuenta.";
        }

        public async Task<List<UsuarioDTO>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var propiedades = await _propiedadRepository.GetAllAsync();
            var userDTOs = new List<UsuarioDTO>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var dto = new UsuarioDTO
                {
                    Id = user.Id,
                    Nombre = user.Nombre,
                    Apellido = user.Apellido,
                    Email = user.Email,
                    UserName = user.UserName,
                    Cedula = user.Cedula,
                    Telefono = user.Telefono,
                    FotoPerfilUrl = user.FotoPerfilUrl,
                    EstaActivo = user.EstaActivo,
                    Roles = roles.ToList()
                };

                if (roles.Contains("Agente"))
                    dto.CantidadPropiedades = propiedades.Count(p => p.AgenteId == user.Id);

                userDTOs.Add(dto);
            }
            return userDTOs;
        }

        public async Task<UsuarioDTO> GetUserByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            return new UsuarioDTO
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                Email = user.Email,
                UserName = user.UserName,
                Cedula = user.Cedula,
                Telefono = user.Telefono,
                FotoPerfilUrl = user.FotoPerfilUrl,
                EstaActivo = user.EstaActivo,
                Roles = roles.ToList()
            };
        }

        public async Task UpdateUserAsync(SaveUsuarioDTO request)
        {
            var user = await _userManager.FindByIdAsync(request.Id);
            if (user == null) throw new Exception("Usuario no encontrado.");

            user.Nombre = request.Nombre;
            user.Apellido = request.Apellido;
            user.Telefono = request.Telefono;
            user.Cedula = request.Cedula;

            if (!string.IsNullOrEmpty(request.FotoPerfilUrl))
                user.FotoPerfilUrl = request.FotoPerfilUrl;

            await _userManager.UpdateAsync(user);

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.ResetPasswordAsync(user, token, request.Password);
            }
        }

        public async Task ChangeUserStatusAsync(string userId, bool status, string currentAdminId)
        {
            if (userId == currentAdminId)
                throw new InvalidOperationException("No puedes cambiar tu propio estado.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("Usuario no encontrado.");

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Administrador") && !status)
            {
                var admins = await _userManager.GetUsersInRoleAsync("Administrador");
                var activeAdmins = admins.Count(a => a.EstaActivo);
                if (activeAdmins <= 1)
                    throw new InvalidOperationException("Debe haber al menos un administrador activo.");
            }

            user.EstaActivo = status;
            await _userManager.UpdateAsync(user);
        }

        public async Task DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) throw new Exception("Usuario no encontrado.");

            var propiedadesDelAgente = (await _propiedadRepository.GetAllAsync())
                                        .Where(p => p.AgenteId == userId).ToList();

            foreach (var prop in propiedadesDelAgente)
            {
                await _propiedadRepository.DeleteConDependenciasAsync(prop.Id);
            }

            await _userManager.DeleteAsync(user);
        }

        public async Task<string> SendVerificationEmailAsync(string emailOrUserName, string origin)
        {
            var user = await _userManager.FindByEmailAsync(emailOrUserName) ??
                       await _userManager.FindByNameAsync(emailOrUserName);

            if (user == null)
            {
                return "No se encontró ningún usuario registrado con esos datos.";
            }

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                return "Este usuario ya ha confirmado su correo electrónico.";
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var activationUrl = $"{origin}/Account/ConfirmEmail?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            string cuerpoCorreo = $@"
        <h3>Hola {user.Nombre},</h3>
        <p>Ha solicitado un nuevo enlace para activar su cuenta en RealEstateApp.</p>
        <p>Para activar su usuario y poder iniciar sesión, utilice el siguiente enlace:</p>
        <p><a href='{activationUrl}' style='padding: 10px 18px; background-color: #0d6efd; color: white; text-decoration: none; border-radius: 5px; font-weight: bold;'>Activar Cuenta</a></p>
        <p>Si no puede hacer clic en el botón, copie y pegue la siguiente URL en su navegador:</p>
        <p>{activationUrl}</p>
        <br/>
        <p><small>Si usted no solicitó este cambio, puede ignorar este mensaje.</small></p>";

            await _emailService.SendAsync(
                user.Email,
                "Reenvío - Activación de cuenta en RealEstateApp",
                cuerpoCorreo);

            return "Success";
        }
    }
}