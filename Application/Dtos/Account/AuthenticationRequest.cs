namespace RealStateApp.Core.Application.DTOs.Account
{
    public class AuthenticationRequest
    {
        public string EmailOrUserName { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}