namespace RealStateApp.Core.Application.ViewModels.Admin
{
    public class AgentListViewModel
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public int CantidadPropiedades { get; set; }
        public bool EstaActivo { get; set; }
    }
}