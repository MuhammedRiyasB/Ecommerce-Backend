using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Infrastructure.Data
{
    /// <summary>
    /// Seeds the database with initial data (e.g., admin user).
    /// </summary>
    public static class DbSeeder
    {
        public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

            var adminEmail = configuration["AdminSettings:Email"];
            var adminPassword = configuration["AdminSettings:Password"];

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                logger.LogWarning("Admin seed skipped: AdminSettings:Email/Password not configured.");
                return;
            }

            if (await context.Users.AnyAsync(u => u.Email == adminEmail))
            {
                logger.LogInformation("Admin user already exists with email: {Email}", adminEmail);
                return;
            }

            var admin = new User
            {
                UserId = Guid.NewGuid(),
                Name = "Admin",
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                Role = UserRole.Admin,
                IsBlocked = false
            };

            context.Users.Add(admin);
            await context.SaveChangesAsync();
            logger.LogInformation("Admin user seeded successfully with email: {Email}", adminEmail);
        }
    }
}
