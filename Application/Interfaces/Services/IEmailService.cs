using System.Threading.Tasks;

namespace RealStateApp.Core.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string body);
    }
}