using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ITIGraduationProject.Application
{
    public static class ApplicationModuleDependencies
    {
        public static void AddApplicationModuleDependencies(this IServiceCollection services)
        {
            
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
        }
    }
}
