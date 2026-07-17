using System.Collections.Generic;

namespace RealStateApp.Core.Application.DTOs.Usuarios
{
    public class UsuarioDTO
    {
        public string Id { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string Apellido { get; set; } = null!;
        public string Telefono { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? Cedula { get; set; }
        public string? FotoPerfilUrl { get; set; }
        public bool EstaActivo { get; set; }
        public List<string> Roles { get; set; } = new();
        public int CantidadPropiedades { get; set; } 
    }
}