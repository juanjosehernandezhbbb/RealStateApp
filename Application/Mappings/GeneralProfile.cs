using AutoMapper;
using RealStateApp.Core.Application.DTOs.Favoritos;
using RealStateApp.Core.Application.DTOs.Mejoras;
using RealStateApp.Core.Application.DTOs.Mensajes;
using RealStateApp.Core.Application.DTOs.Ofertas;
using RealStateApp.Core.Application.DTOs.Propiedades;
using RealStateApp.Core.Application.DTOs.TipoPropiedades;
using RealStateApp.Core.Application.DTOs.TipoVentas;
using RealStateApp.Core.Domain.Entities;
using RealStateApp.Core.Domain.Enums;

namespace RealStateApp.Core.Application.Mappings
{
    public class GeneralProfile : Profile
    {
        public GeneralProfile()
        {
            #region Tipo de Propiedades
            CreateMap<TipoPropiedad, TipoPropiedadDTO>()
                .ForMember(dest => dest.CantidadPropiedades, opt => opt.MapFrom(src => src.Propiedades.Count))
                .ReverseMap()
                .ForMember(dest => dest.Propiedades, opt => opt.Ignore());

            CreateMap<TipoPropiedad, SaveTipoPropiedadDTO>().ReverseMap()
                .ForMember(dest => dest.Propiedades, opt => opt.Ignore());
            #endregion

            #region Tipo de Ventas
            CreateMap<TipoVenta, TipoVentaDTO>()
                .ForMember(dest => dest.CantidadPropiedades, opt => opt.MapFrom(src => src.Propiedades.Count))
                .ReverseMap()
                .ForMember(dest => dest.Propiedades, opt => opt.Ignore());

            CreateMap<TipoVenta, SaveTipoVentaDTO>().ReverseMap()
                .ForMember(dest => dest.Propiedades, opt => opt.Ignore());
            #endregion

            #region Mejoras
            CreateMap<Mejora, MejoraDTO>()
                .ForMember(dest => dest.CantidadPropiedades, opt => opt.MapFrom(src => src.PropiedadMejoras.Count))
                .ReverseMap()
                .ForMember(dest => dest.PropiedadMejoras, opt => opt.Ignore());

            CreateMap<Mejora, SaveMejoraDTO>().ReverseMap()
                .ForMember(dest => dest.PropiedadMejoras, opt => opt.Ignore());
            #endregion

            #region Propiedades
            CreateMap<Propiedad, PropiedadDTO>()
                .ForMember(dest => dest.TipoPropiedadNombre, opt => opt.MapFrom(src => src.TipoPropiedad.Nombre))
                .ForMember(dest => dest.TipoVentaNombre, opt => opt.MapFrom(src => src.TipoVenta.Nombre))
                .ForMember(dest => dest.AgenteNombre, opt => opt.MapFrom(src => $"{src.Agente.Nombre} {src.Agente.Apellido}"))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado.ToString()))
                .ForMember(dest => dest.ImagenesUrls, opt => opt.MapFrom(src => src.Imagenes.Select(i => i.ImagenUrl).ToList()))
                .ForMember(dest => dest.Mejoras, opt => opt.MapFrom(src => src.PropiedadMejoras.Select(pm => pm.Mejora.Nombre).ToList()))
                .ForMember(dest => dest.MejorasIds, opt => opt.MapFrom(src => src.PropiedadMejoras.Select(pm => pm.MejoraId).ToList()));

            CreateMap<Propiedad, SavePropiedadDTO>()
                .ForMember(dest => dest.MejorasIds, opt => opt.MapFrom(src => src.PropiedadMejoras.Select(pm => pm.MejoraId).ToList()))
                .ForMember(dest => dest.ImagenesUrls, opt => opt.MapFrom(src => src.Imagenes.Select(i => i.ImagenUrl).ToList()))
                .ReverseMap()
                .ForMember(dest => dest.Estado, opt => opt.Ignore())
                .ForMember(dest => dest.Codigo, opt => opt.Ignore())
                .ForMember(dest => dest.PropiedadMejoras, opt => opt.MapFrom(src =>
                    src.MejorasIds != null
                        ? src.MejorasIds.Select(id => new PropiedadMejora { MejoraId = id }).ToList()
                        : new List<PropiedadMejora>())) // <-- Protección contra NULL
                .ForMember(dest => dest.Imagenes, opt => opt.MapFrom(src =>
                    src.ImagenesUrls != null
                        ? src.ImagenesUrls.Select(url => new PropiedadImagen { ImagenUrl = url }).ToList()
                        : new List<PropiedadImagen>())); // <-- Protección contra NULL
            #endregion

            #region Ofertas
            CreateMap<Oferta, OfertaDTO>()
                .ForMember(dest => dest.CompradorNombre, opt => opt.MapFrom(src => $"{src.Comprador.Nombre} {src.Comprador.Apellido}"))
                .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado.ToString()));

            CreateMap<SaveOfertaDTO, Oferta>()
                .ForMember(dest => dest.Estado, opt => opt.Ignore())
                .ReverseMap(); // <-- Agregando esto habilitas el mapeo de Oferta -> SaveOfertaDTO
            #endregion

            #region Mensajes
            CreateMap<Mensaje, MensajeDTO>()
                .ForMember(dest => dest.ClienteNombre, opt => opt.MapFrom(src => $"{src.Cliente.Nombre} {src.Cliente.Apellido}"))
                .ForMember(dest => dest.AgenteNombre, opt => opt.MapFrom(src => $"{src.Agente.Nombre} {src.Agente.Apellido}"));

            CreateMap<SaveMensajeDTO, Mensaje>().ReverseMap();
            #endregion

            #region Favoritos
            CreateMap<Favorito, FavoritoDTO>()
                .ForMember(dest => dest.Propiedad, opt => opt.MapFrom(src => src.Favorita))
                .ReverseMap()
                .ForMember(dest => dest.Favorita, opt => opt.Ignore());

            CreateMap<SaveFavoritoDTO, Favorito>().ReverseMap();
            #endregion

        }
    }
}