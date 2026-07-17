using System.Collections.Generic;

namespace RealStateApp.Core.Application.DTOs.Account
{
    public class AuthenticationResponse
    {
        public string Id { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public List<string> Roles { get; set; } = new();
        public bool IsActive { get; set; }
        public string? Token { get; set; }
        public bool HasError { get; set; }
        public string? Error { get; set; }
    }
}