using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ITIGraduationProject.Application
{
    public static class ApplicationModuleDependencies
    {
        public static void AddApplicationModuleDependencies(this IServiceCollection services)
        {
            
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
            // Add Mapster
            var config = TypeAdapterConfig.GlobalSettings;
            //config.Apply(new ShopMappingConfig());
            //config.Apply(new TemplateMappingConfig());
            //config.Apply(new CommunityMappingConfig());
            //services.AddSingleton(config);
            //services.AddScoped<IMapper, ServiceMapper>();
        }
    }
}
