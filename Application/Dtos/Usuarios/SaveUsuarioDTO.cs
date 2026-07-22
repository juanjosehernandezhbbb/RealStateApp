using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace RealStateApp.Core.Application.DTOs.Usuarios
{
    public class SaveUsuarioDTO
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Debe completar todos los campos requeridos.")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "Debe completar todos los campos requeridos.")]
        public string Apellido { get; set; } = null!;

        [Required(ErrorMessage = "Debe completar todos los campos requeridos.")]
        public string Telefono { get; set; } = null!;

        [Required(ErrorMessage = "Debe completar todos los campos requeridos.")]
        [EmailAddress(ErrorMessage = "Debe ingresar un correo electrónico válido.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Debe completar todos los campos requeridos.")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Debe completar todos los campos requeridos.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Debe completar todos los campos requeridos.")]
        [Compare("Password", ErrorMessage = "La contraseña y la confirmación de contraseña no coinciden.")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }

        public string? Cedula { get; set; }

        public string? FotoPerfilUrl { get; set; }

        [Required(ErrorMessage = "Debe completar todos los campos requeridos.")]
        public IFormFile? File { get; set; }

        [Required(ErrorMessage = "Debe completar todos los campos requeridos.")]
        public string Rol { get; set; } = null!;

        public bool EstaActivo { get; set; }
    }
}