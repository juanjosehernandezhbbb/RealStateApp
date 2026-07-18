using Microsoft.Extensions.DependencyInjection;
using RealStateApp.Core.Application.Interfaces.Services;
using RealStateApp.Core.Application.Mappings;
using RealStateApp.Core.Application.Services;
using System.Reflection;

namespace RealStateApp.Core.Application
{
    public static class ServiceRegistration
    {
        public static void AddApplicationLayer(this IServiceCollection services)
        {
            // Registrar AutoMapper
            services.AddAutoMapper(config =>
            {
                config.AddProfile<GeneralProfile>();
            });

            // Registrar Servicios Específicos
            services.AddTransient<ITipoPropiedadService, TipoPropiedadService>();
            services.AddTransient<ITipoVentaService, TipoVentaService>();
            services.AddTransient<IMejoraService, MejoraService>();
            services.AddTransient<IPropiedadService, PropiedadService>();
            services.AddTransient<IOfertaService, OfertaService>();
            services.AddTransient<IMensajeService, MensajeService>();
            services.AddTransient<IFavoritoService, FavoritoService>();
        }
    }
}