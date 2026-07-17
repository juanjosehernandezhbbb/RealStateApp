using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RealStateApp.Core.Application.Interfaces.Services;
using RealStateApp.Core.Application.Settings;
using RealStateApp.Persistance.Identity.Services;

namespace RealStateApp.Persistance.Identity
{
    public static class ServiceRegistration
    {
        public static void AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Registrar las configuraciones de JWT mapeándolas desde el appsettings.json
            services.Configure<JWTSettings>(configuration.GetSection("JWTSettings"));

            // Registrar el servicio de cuentas
            services.AddTransient<IAccountService, AccountService>();
        }
    }
}