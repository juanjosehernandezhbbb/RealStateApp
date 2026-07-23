namespace RealStateApp.Core.Application.DTOs.Account
{
    public class RegisterApiRequest
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Cedula { get; set; }
        public string Telefono { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Rol { get; set; }
    }
}