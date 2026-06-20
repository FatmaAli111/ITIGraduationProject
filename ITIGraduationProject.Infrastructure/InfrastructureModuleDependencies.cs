using ITIGraduationProject.Application.Interfaces;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Interfaces.Repositories;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities;
using ITIGraduationProject.Infrastructure.Identity;
using ITIGraduationProject.Infrastructure.Persistence;
using ITIGraduationProject.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Infrastructure
{
    public static class InfrastructureModuleDependencies
    {

        public static void AddInfrastructureModuleDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
           options.UseSqlServer(configuration.GetConnectionString("DefaultConnectionString"))
           );
            
            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options=>
          {
              options.Lockout.AllowedForNewUsers = true;
              options.Lockout.MaxFailedAccessAttempts = 5;
              options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
          }  )
                .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // Authentication (JWT + External Login Google & Facebook)

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
       .AddJwtBearer(options =>
       {
           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateIssuer = true,
               ValidateAudience = true,
               ValidateLifetime = true,
               ValidateIssuerSigningKey = true,

               ValidIssuer = configuration["JwtSettings:Issuer"],
               ValidAudience = configuration["JwtSettings:Audience"],

               IssuerSigningKey = new SymmetricSecurityKey(
                   Encoding.UTF8.GetBytes(
                       configuration["JwtSettings:SecretKey"]))
           };
       })
       .AddGoogle(options =>
       {
           options.ClientId =
               configuration["Authentication:Google:ClientId"];

           options.ClientSecret =
               configuration["Authentication:Google:ClientSecret"];
       });
            //.AddFacebook(options =>
            //{
            //    options.AppId =
            //        configuration["Authentication:Facebook:AppId"];

            //    //options.AppSecret =
            //    //    configuration["Authentication:Facebook:Facebook:AppSecret"];
            //});
            services.AddHttpClient<IAILayerClient, AILayerClient>(client =>
            {
                client.BaseAddress = new Uri(configuration["AILayer:BaseUrl"]!);
                client.Timeout = TimeSpan.FromSeconds(300);
                client.DefaultRequestHeaders.Add("x-api-key", configuration["AILayer:ApiKey"]);
            });
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

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
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}
