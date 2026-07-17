using RealStateApp.Core.Application.DTOs.Account;
using RealStateApp.Core.Application.DTOs.Usuarios;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealStateApp.Core.Application.Interfaces.Services
{
    public interface IAccountService
    {
        Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request);
        Task<string> RegisterUserAsync(SaveUsuarioDTO request, string origin);
        Task<string> ConfirmAccountAsync(string userId, string token);
        Task SignOutAsync();

        Task<List<UsuarioDTO>> GetAllUsersAsync();
        Task<UsuarioDTO> GetUserByIdAsync(string id);
        Task UpdateUserAsync(SaveUsuarioDTO request);
        Task ChangeUserStatusAsync(string userId, bool status);
        Task DeleteUserAsync(string userId);
    }
}