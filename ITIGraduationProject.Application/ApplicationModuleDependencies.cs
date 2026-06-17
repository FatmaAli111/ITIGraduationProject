using FluentValidation;
using ITIGraduationProject.Application.Features.Identitiy.Commands.Handlers;
using ITIGraduationProject.Application.Features.Identitiy.Commands.Validators.ITIGraduationProject.Application.Behaviors;
using ITIGraduationProject.Application.Features.Identity.Commands.Validators;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ITIGraduationProject.Application
{
    public static class ApplicationModuleDependencies
    {
        public static void AddApplicationModuleDependencies(this IServiceCollection services)
        {

            services.AddMediatR(cfg =>
               cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            var config = TypeAdapterConfig.GlobalSettings;

            config.Scan(Assembly.GetExecutingAssembly());

            services.AddSingleton(config);
            services.AddMapster();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddValidatorsFromAssembly(typeof(RegisterCommandValidator).Assembly);
        }
    }
}
