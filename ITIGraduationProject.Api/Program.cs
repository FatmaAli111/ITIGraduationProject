using ITIGraduationProject.Application;
using ITIGraduationProject.Infrastructure;
using ITIGraduationProject.Service;

namespace ITIGraduationProject.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            //Add Module Dependencies
            builder.Services.AddServiceModuleDependencies();
            builder.Services.AddInfrastructureModuleDependencies();
            builder.Services.AddApplicationModuleDependencies();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
