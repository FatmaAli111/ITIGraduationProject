using ITIGraduationProject.Domain.Entities;
using ITIGraduationProject.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITIGraduationProject.Infrastructure.Identity;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Infrastructure.Persistence.Repositories;
using ITIGraduationProject.Application.Interfaces.Persistence;

namespace ITIGraduationProject.Infrastructure
{
    public static class InfrastructureModuleDependencies
    {

        public static void AddInfrastructureModuleDependencies(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            services.AddScoped(typeof(IGenericRepo<>), typeof(GenericRepo<>));

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IDesignRepository, DesignRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITemplateRepository, TemplateRepository>();
            services.AddScoped<ICouponRepository, CouponRepository>();
            services.AddScoped<IModerationReportRepository, ModerationReportRepository>();
            services.AddScoped<IAiChatSessionRepository, AiChatSessionRepository>();
            services.AddScoped<IShipmentRepository, ShipmentRepository>();
            services.AddScoped<IRewardRepository, RewardRepository>();
            services.AddScoped<IGraphicAssetRepository, GraphicAssetRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}
