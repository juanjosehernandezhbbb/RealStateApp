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

            var result = await _signInManager.PasswordSignInAsync(user.UserName, request.Password, false, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Error = "Los datos de acceso son inválidos.";
                return response;
            }

            if (!user.EstaActivo)
            {
                response.HasError = true;
                response.Error = "El usuario se encuentra inactivo y no puede iniciar sesión.";
                return response;
            }

            var rolesList = await _userManager.GetRolesAsync(user);
            response.Id = user.Id;
            response.Email = user.Email;
            response.UserName = user.UserName;
            response.Roles = rolesList.ToList();
            response.IsActive = user.EstaActivo;

            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
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

            var token = tokenHandler.CreateToken(tokenDescriptor);
            response.Token = tokenHandler.WriteToken(token);

            return response;
        }

        public async Task<string> RegisterUserAsync(SaveUsuarioDTO request, string origin)
        {
            if (await _userManager.Users.AnyAsync(u => u.Cedula == request.Cedula))
                return "Ya existe un usuario registrado con esta cédula.";

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

                EstaActivo = request.Rol == "Administrador" || request.Rol == "Desarrollador"
            };
            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, request.Rol);

                if (request.Rol == "Cliente")
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    await _emailService.SendAsync(
                        user.Email,
                        "Confirmación de Cuenta",
                        $"Su cuenta ha sido creada. Utilice el siguiente token para confirmarla: {token}");
                }

                return "Success";
            }
            return result.Errors.FirstOrDefault()?.Description;
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
    }
}