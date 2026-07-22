namespace RealStateApp.Core.Application.ViewModels.Admin
{
    public class AdminListViewModel
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string UserName { get; set; }
        public string Cedula { get; set; }
        public string Email { get; set; }
        public bool EstaActivo { get; set; }
    }
}