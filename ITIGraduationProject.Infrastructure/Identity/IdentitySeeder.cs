using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ITIGraduationProject.Domain.Constants;
using ITIGraduationProject.Domain.Entities.Identity;
using ITIGraduationProject.Domain.Entities.ECommerce;
using System.Data;
using ITIGraduationProject.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ITIGraduationProject.Infrastructure.Identity
{

    public static class IdentitySeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
                .CreateLogger("IdentitySeeder");

            await SeedRolesAsync(roleManager, logger);
            await SeedAdminAsync(userManager, context, configuration, logger);
            await SeedPrinterAsync(userManager, context, configuration, logger);
            await EnsureDomainUserIdentityFieldsAsync(context, logger);

            await ApplicationSeeder.SeedAsync(scope.ServiceProvider);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager, ILogger logger)
        {
            string[] roles = { Roles.Admin, Roles.User, Roles.Printer };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                    logger.LogInformation("Role '{Role}' created.", role);
                }
            }
        }
        private static async Task SeedAdminAsync(
            UserManager<ApplicationUser> userManager,
            AppDbContext context,
            IConfiguration configuration,
            ILogger logger)
        {
            var email = configuration["AdminSeed:Email"];
            var password = configuration["AdminSeed:Password"];
            var name = configuration["AdminSeed:Name"] ?? "System Admin";

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning("AdminSeed configuration is missing. Skipping admin seeding.");
                return;
            }

            var applicationUser = await userManager.FindByEmailAsync(email);

            if (applicationUser is null)
            {
                var newId = Guid.NewGuid();

                var domainUser = new User
                {
                    Id = newId,
                    Name = name,
                    Email = email,
                    UserName = email,
                    IsActive = true,
                    CurrentPointsBalance = 0,
                    UserPreferences = new UserPreferences(),
                    Cart = new Cart()
                };

                context.AppUsers.Add(domainUser);
                await context.SaveChangesAsync();

                applicationUser = new ApplicationUser
                {
                    Id = newId,
                    Email = email,
                    UserName = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(applicationUser, password);
                if (!result.Succeeded)
                {
                    context.AppUsers.Remove(domainUser);
                    await context.SaveChangesAsync();

                    logger.LogError("Failed to create admin user: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                    return;
                }

                logger.LogInformation("Admin ApplicationUser '{Email}' created.", email);
            }
            else if (!applicationUser.EmailConfirmed)
            {
                applicationUser.EmailConfirmed = true;
                await userManager.UpdateAsync(applicationUser);
            }

            if (!await userManager.IsInRoleAsync(applicationUser, Roles.Admin))
            {
                await userManager.AddToRoleAsync(applicationUser, Roles.Admin);
                logger.LogInformation("Admin role assigned to '{Email}'.", email);
            }

            var domainUserExists = await context.AppUsers
                .AnyAsync(u => u.Id == applicationUser.Id);

            if (!domainUserExists)
            {
                var domainUser = new User
                {
                    Id = applicationUser.Id,
                    Name = name,
                    Email = email,
                    UserName = email,
                    IsActive = true,
                    CurrentPointsBalance = 0,
                    UserPreferences = new UserPreferences(),
                    Cart = new Cart()
                };

                context.AppUsers.Add(domainUser);
                await context.SaveChangesAsync();

                logger.LogInformation("Domain User created for admin '{Email}'.", email);
            }
        }
        private static async Task SeedPrinterAsync(
    UserManager<ApplicationUser> userManager,
    AppDbContext context,
    IConfiguration configuration,
    ILogger logger)
        {
            var email = configuration["PrinterSeed:Email"];
            var password = configuration["PrinterSeed:Password"];
            var name = configuration["PrinterSeed:Name"] ?? "Default Printer";

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning("PrinterSeed configuration is missing. Skipping printer seeding.");
                return;
            }

            var applicationUser = await userManager.FindByEmailAsync(email);

            if (applicationUser is null)
            {
                var newId = Guid.NewGuid();

                var domainUser = new User
                {
                    Id = newId,
                    Name = name,
                    Email = email,
                    UserName = email,
                    IsActive = true,
                    CurrentPointsBalance = 0,
                    UserPreferences = new UserPreferences(),
                    Cart = new Cart()
                };

                context.AppUsers.Add(domainUser);
                await context.SaveChangesAsync();

                applicationUser = new ApplicationUser
                {
                    Id = newId,
                    Email = email,
                    UserName = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(applicationUser, password);

                if (!result.Succeeded)
                {
                    context.AppUsers.Remove(domainUser);
                    await context.SaveChangesAsync();

                    logger.LogError(
                        "Failed to create printer user: {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));

                    return;
                }

                logger.LogInformation("Printer ApplicationUser '{Email}' created.", email);
            }
            else if (!applicationUser.EmailConfirmed)
            {
                applicationUser.EmailConfirmed = true;
                await userManager.UpdateAsync(applicationUser);
            }

            if (!await userManager.IsInRoleAsync(applicationUser, Roles.Printer))
            {
                await userManager.AddToRoleAsync(applicationUser, Roles.Printer);
                logger.LogInformation("Printer role assigned to '{Email}'.", email);
            }

            var domainUserExists = await context.AppUsers
                .AnyAsync(u => u.Id == applicationUser.Id);

            if (!domainUserExists)
            {
                var domainUser = new User
                {
                    Id = applicationUser.Id,
                    Name = name,
                    Email = email,
                    UserName = email,
                    IsActive = true,
                    CurrentPointsBalance = 0,
                    UserPreferences = new UserPreferences(),
                    Cart = new Cart()
                };

                context.AppUsers.Add(domainUser);
                await context.SaveChangesAsync();

                logger.LogInformation("Domain User created for printer '{Email}'.", email);
            }
        }

        private static async Task EnsureDomainUserIdentityFieldsAsync(
            AppDbContext context,
            ILogger logger)
        {
            try
            {
                var domainUsers = await context.AppUsers
                    .Where(user =>
                        user.Email == null || user.Email == "" ||
                        user.UserName == null || user.UserName == "")
                    .ToListAsync();

                logger.LogInformation(
                    "Domain identity backfill found {UserCount} user row(s) with missing Email or UserName.",
                    domainUsers.Count);

                if (domainUsers.Count == 0)
                {
                    logger.LogInformation("Domain identity backfill saved 0 updated user row(s).");
                    return;
                }

                var userIds = domainUsers.Select(user => user.Id).ToArray();
                var identityUsers = await context.Users
                    .Where(user => userIds.Contains(user.Id))
                    .ToDictionaryAsync(user => user.Id);
                var updatedCount = 0;

                foreach (var domainUser in domainUsers)
                {
                    logger.LogInformation(
                        "Domain identity backfill processing user {UserId}.",
                        domainUser.Id);

                    if (!identityUsers.TryGetValue(domainUser.Id, out var identityUser))
                    {
                        logger.LogWarning(
                            "Domain identity backfill skipped user {UserId}: no matching AspNetUsers row.",
                            domainUser.Id);
                        continue;
                    }

                    var updated = false;

                    if (string.IsNullOrWhiteSpace(domainUser.Email) &&
                        !string.IsNullOrWhiteSpace(identityUser.Email))
                    {
                        domainUser.Email = identityUser.Email;
                        updated = true;
                    }

                    if (string.IsNullOrWhiteSpace(domainUser.UserName) &&
                        !string.IsNullOrWhiteSpace(identityUser.UserName))
                    {
                        domainUser.UserName = identityUser.UserName;
                        updated = true;
                    }

                    if (!updated)
                    {
                        logger.LogWarning(
                            "Domain identity backfill skipped user {UserId}: matching identity fields are empty.",
                            domainUser.Id);
                        continue;
                    }

                    updatedCount++;
                }

                if (updatedCount > 0)
                {
                    await context.SaveChangesAsync();
                }

                logger.LogInformation(
                    "Domain identity backfill saved {UpdatedCount} updated user row(s).",
                    updatedCount);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Domain identity backfill failed.");
                throw;
            }
        }
    }
}
