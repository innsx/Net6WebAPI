using Application.Interfaces;
using Infastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infastructure
{
    public static class ServiceExtensions
    {
        public static void AppInfrastructure(this IServiceCollection services)
        {

            services.AddTransient<IEmailService, EmailService>();
        }
    }
}
