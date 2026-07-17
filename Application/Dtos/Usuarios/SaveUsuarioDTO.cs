namespace RealStateApp.Core.Application.DTOs.Usuarios
{
    public class SaveUsuarioDTO
    {
        public string? Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Apellido { get; set; } = null!;
        public string Telefono { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? Password { get; set; }
        public string? Cedula { get; set; }

        public string? FotoPerfilUrl { get; set; }

        public string Rol { get; set; } = null!; 
        public bool EstaActivo { get; set; }
    }
}