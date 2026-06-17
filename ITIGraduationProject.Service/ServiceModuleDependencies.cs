using ITIGraduationProject.Application.Interfaces.IServices.IdentityServices;
using ITIGraduationProject.Service.Identity.Authantication;
using ITIGraduationProject.Service.Identity.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ITIGraduationProject.Service
{
    public static class ServiceModuleDependencies
    {
        public static void AddServiceModuleDependencies(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IEmailService, EmailService>();
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

        }
    }
}
