using ITIGraduationProject.Domain.Entities;
using ITIGraduationProject.Infrastructure.Data;
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

namespace ITIGraduationProject.Infrastructure
{
    public static class InfrastructureModuleDependencies
    {

        public static void AddInfrastructureModuleDependencies(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole<string>>()
                .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
        }
    }
}
