using Microsoft.AspNetCore.Identity;
using RealStateApp.Core.Application.DTOs.Account;
using RealStateApp.Core.Application.DTOs.Usuarios;
using RealStateApp.Core.Application.Interfaces.Services;
using RealStateApp.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RealStateApp.Core.Application.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RealStateApp.Persistance.Identity.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly JWTSettings _jwtSettings;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            IOptions<JWTSettings> jwtSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _jwtSettings = jwtSettings.Value;
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

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            byte[] key = Encoding.ASCII.GetBytes(_jwtSettings.Key);

            List<Claim> claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id)
            };

            foreach (var role in rolesList)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            response.Token = tokenHandler.WriteToken(token);

            return response;
        }

        public async Task<string> RegisterUserAsync(SaveUsuarioDTO request, string origin)
        {
            var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userWithSameEmail != null)
                return "Ya existe un usuario registrado con este correo electrónico.";

            var userWithSameUserName = await _userManager.FindByNameAsync(request.UserName);
            if (userWithSameUserName != null)
                return "Ya existe un usuario registrado con este nombre de usuario.";

            bool isActivo = request.Rol == "Administrador" || request.Rol == "Desarrollador";

            var user = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.UserName,
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Telefono = request.Telefono,
                Cedula = request.Cedula,
                FotoPerfilUrl = request.FotoPerfilUrl,
                EstaActivo = isActivo,
                FechaRegistro = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return "Ocurrió un error al registrar el usuario.";

            await _userManager.AddToRoleAsync(user, request.Rol);

            if (request.Rol == "Cliente")
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var verificationUri = $"{origin}/User/ConfirmEmail?userId={user.Id}&token={token}";

                string body = $"Hola {user.Nombre}, <br/><br/> Su cuenta ha sido registrada correctamente en RealEstateApp.<br/>" +
                              $"Para activar su usuario y poder iniciar sesión, utilice el siguiente enlace: <br/>" +
                              $"<a href='{verificationUri}'>Activar Cuenta</a>";

                await _emailService.SendAsync(user.Email, "Activación de cuenta en RealEstateApp", body);
            }

            return "Success";
        }

        public async Task<string> ConfirmAccountAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return "Usuario no encontrado.";

            token = token.Replace(" ", "+");
            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                user.EstaActivo = true;
                await _userManager.UpdateAsync(user);
                return "Success";
            }

            return "Ocurrió un error confirmando la cuenta.";
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public Task<List<UsuarioDTO>> GetAllUsersAsync() => throw new NotImplementedException();
        public Task<UsuarioDTO> GetUserByIdAsync(string id) => throw new NotImplementedException();
        public Task UpdateUserAsync(SaveUsuarioDTO request) => throw new NotImplementedException();
        public Task ChangeUserStatusAsync(string userId, bool status) => throw new NotImplementedException();
        public Task DeleteUserAsync(string userId) => throw new NotImplementedException();
    }
}