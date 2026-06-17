using ITIGraduationProject.Application;
using ITIGraduationProject.Application.Middlewares;
using ITIGraduationProject.Infrastructure;
using ITIGraduationProject.Infrastructure.Identity;
using ITIGraduationProject.Infrastructure.Persistence;
using ITIGraduationProject.Service;
using Microsoft.EntityFrameworkCore;

namespace ITIGraduationProject.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            //add dbcontext

            // adding swagger services to run Http
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString"))
            );
            //Add Module Dependencies
            builder.Services.AddServiceModuleDependencies(builder.Configuration);
            builder.Services.AddInfrastructureModuleDependencies();
            builder.Services.AddApplicationModuleDependencies();
            
            var app = builder.Build();
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            IdentitySeeder.SeedAsync(app.Services);

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Wearly API V1");
            });

            // Configure the HTTP request pipeline.
            app.UseMiddleware<ErrorHandlerMiddleware>();

            app.UseHttpsRedirection();
            //app.UseCors(CORS);

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
