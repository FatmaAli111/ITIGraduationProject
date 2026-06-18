using ITIGraduationProject.Application;
using ITIGraduationProject.Application.Middlewares;
using ITIGraduationProject.Infrastructure;
using ITIGraduationProject.Infrastructure.Identity;
using ITIGraduationProject.Infrastructure.Persistence;
using ITIGraduationProject.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

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

            //Add Module Dependencies
            builder.Services.AddServiceModuleDependencies(builder.Configuration);
            builder.Services.AddInfrastructureModuleDependencies(builder.Configuration);
            builder.Services.AddApplicationModuleDependencies();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your token"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
            });

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

            app.UseAuthentication();  
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
