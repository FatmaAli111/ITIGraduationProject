using ITIGraduationProject.Application.Interfaces; 
using ITIGraduationProject.Application.Interfaces.IAiService;
using ITIGraduationProject.Application.Interfaces.IServices.AdminIServices;
using ITIGraduationProject.Application.Interfaces.IServices.FilesServices;
using ITIGraduationProject.Application.Interfaces.IServices.IdentityServices;
using ITIGraduationProject.Application.Interfaces.IServices.IReportServices.ITIGraduationProject.Application.Interfaces.IServices;
using ITIGraduationProject.Application.Interfaces.IServices.IReportServices;
using ITIGraduationProject.Application.Interfaces.IServices.Notification;
using ITIGraduationProject.Application.Interfaces.IServices.StudioServices;
using ITIGraduationProject.Service.Admin;
using ITIGraduationProject.Service.AI;
using ITIGraduationProject.Service.FileService;
using ITIGraduationProject.Service.Identity.Authantication;
using ITIGraduationProject.Service.Identity.Email;
using ITIGraduationProject.Service.Identity.JWT;
using ITIGraduationProject.Service.NotificationServices;
using ITIGraduationProject.Service.ReportGenerator;
using ITIGraduationProject.Service.Studio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ITIGraduationProject.Service
{
    public static class ServiceModuleDependencies
    {
        public static void AddServiceModuleDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IEmailService, EmailService>();
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IAdminUserService, AdminUserService>();
            services.AddScoped<IPriceCalculation, PriceCalculationService>();
            services.AddScoped<IFileService, LocalFileService>();
            services.AddHttpClient<IAiService, AiService>();
            services.AddScoped<IReportChatService,ReportChatService>();
            services.AddHttpClient<ILangflowService,LangflowService>(client =>
                {
                    client.BaseAddress =
            new Uri("http://localhost:7860/");
                });
        }
    }
}