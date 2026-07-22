using System.ComponentModel.DataAnnotations;

namespace RealStateApp.Core.Application.ViewModels.Admin
{
    public class UpdateDeveloperViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es requerido.")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "La cédula es requerida.")]
        public string Cedula { get; set; }

        [Required(ErrorMessage = "El correo electrónico es requerido.")]
        [EmailAddress(ErrorMessage = "El correo electrónico debe tener un formato válido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es requerido.")]
        public string UserName { get; set; }

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "La contraseña y la confirmación de contraseña no coinciden.")]
        public string? ConfirmPassword { get; set; }
    }
}