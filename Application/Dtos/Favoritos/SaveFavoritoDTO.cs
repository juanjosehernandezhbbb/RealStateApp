namespace RealStateApp.Core.Application.DTOs.Favoritos
{
    public class SaveFavoritoDTO
    {
        public int Id { get; set; }
        public string UsuarioId { get; set; } = null!;
        public int PropiedadFavoritaId { get; set; }
    }
}