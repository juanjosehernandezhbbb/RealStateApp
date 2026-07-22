namespace RealStateApp.Core.Application.ViewModels
{
    public class PropertyViewModel
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
        public int TipoPropiedadId { get; set; }
        public string TipoPropiedad { get; set; }
        public int TipoVentaId { get; set; }
        public string TipoVenta { get; set; }
        public decimal Precio { get; set; }
        public int CantidadHabitaciones { get; set; }
        public int CantidadBanos { get; set; }
        public double Tamano { get; set; }
        public string ImagenPrincipalUrl { get; set; }
        public string AgenteId { get; set; }
        public string Estado { get; set; }
    }
}