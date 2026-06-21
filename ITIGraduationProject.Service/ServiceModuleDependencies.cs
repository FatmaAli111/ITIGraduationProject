using ITIGraduationProject.Application.Interfaces.IServices.AdminIServices;
using ITIGraduationProject.Application.Interfaces.IServices.IdentityServices;
using ITIGraduationProject.Application.Interfaces.IServices.Notification;
using ITIGraduationProject.Service.Admin;
using ITIGraduationProject.Service.Identity.Authantication;
using ITIGraduationProject.Service.Identity.Email;
using ITIGraduationProject.Service.Identity.JWT;
using ITIGraduationProject.Service.NotificationServices;
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
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<INotificationService,NotificationService>();
            services.AddScoped<IAdminUserService, AdminUserService>();

        }
    }
}
