using ITIGraduationProject.Api.Middlewares;
using ITIGraduationProject.Api.Swagger;
using ITIGraduationProject.Application;
using ITIGraduationProject.Application.Interfaces.IServices.StudioServices;
using ITIGraduationProject.Infrastructure;
using ITIGraduationProject.Infrastructure.Identity;
using ITIGraduationProject.Infrastructure.Persistence;
using ITIGraduationProject.Service;
using ITIGraduationProject.Service.Studio;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using ITIGraduationProject.Infrastructure.SignalR;

namespace ITIGraduationProject.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 52_428_800; // 50 MB
            });

            // Add services to the container.

            builder.Services.AddControllers(options =>
            {
                options.MaxModelBindingCollectionSize = int.MaxValue;
            });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:4200")
                    .AllowAnyMethod()
                        .AllowAnyHeader().AllowCredentials();


                });
            });

            // adding swagger services to run Http
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //Add Module Dependencies
            builder.Services.AddServiceModuleDependencies(builder.Configuration);
            builder.Services.AddInfrastructureModuleDependencies(builder.Configuration);
            builder.Services.AddApplicationModuleDependencies();
            builder.Services.AddScoped<IPriceCalculation, PriceCalculationService>();
            builder.Services.AddSwaggerGen(options =>
            {
                options.MapType<IFormFile>(() => new OpenApiSchema { Type = "string", Format = "binary" });
                options.MapType<IFormFileCollection>(() => new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema { Type = "string", Format = "binary" }
                });
                options.OperationFilter<FileUploadOperationFilter>();
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
            await IdentitySeeder.SeedAsync(app.Services);
            if (app.Environment.IsDevelopment())
            {
                await DemoDataSeeder.SeedAsync(app.Services);
            }
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Wearly API V1");
            });

            // Configure the HTTP request pipeline.
            app.UseMiddleware<ErrorHandlerMiddleware>();

            app.UseHttpsRedirection();
            app.UseCors("AllowFrontend");

            var staticFileOptions = new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                OnPrepareResponse = context =>
                {
                    context.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                    context.Context.Response.Headers.Append("Access-Control-Allow-Headers", "*");
                    context.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET,HEAD,OPTIONS");
                }
            };

            app.UseStaticFiles(staticFileOptions); // serves wwwroot/ contents

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<NotificationHub>("/hubs/notifications");

            app.Run();
        }
    }
}
