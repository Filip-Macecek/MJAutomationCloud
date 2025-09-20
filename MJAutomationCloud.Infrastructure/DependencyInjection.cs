using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MJAutomationCloud.Infrastructure.Data;
using MJAutomationCloud.Infrastructure.Entities;
using MJAutomationCloud.Infrastructure.Interfaces;
using MJAutomationCloud.Infrastructure.Services;

namespace MJAutomationCloud.Infrastructure;

/// <summary>
/// Infrastructure layer dependency injection configuration.
/// Centralizes registration of all infrastructure services following DDD principles.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all infrastructure services with the dependency injection container.
    /// Configures database context, Identity services, and custom infrastructure services.
    /// </summary>
    /// <param name="services">Service collection for dependency registration</param>
    /// <param name="configuration">Application configuration for connection strings and settings</param>
    /// <returns>Service collection for method chaining</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register database context with SQL Server provider
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlite(connectionString);

            // Enable sensitive data logging in development
            if (configuration.GetValue<bool>("Logging:EnableSensitiveDataLogging"))
            {
                options.EnableSensitiveDataLogging();
            }

            // Enable detailed errors in development
            if (configuration.GetValue<bool>("Logging:EnableDetailedErrors"))
            {
                options.EnableDetailedErrors();
            }
        });

        // Note: Identity configuration is handled in the main Program.cs
        // This is because Identity extensions are part of ASP.NET Core framework

        services.AddScoped<IAuthenticationService, AuthenticationService>();

        // Register custom infrastructure services
        services.AddScoped<PasswordResetService>();

        // Add health checks for infrastructure components
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>("database")
            .AddCheck("identity", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Identity system is operational"));

        return services;
    }

    /// <summary>
    /// Ensures the database is created and applies any pending migrations.
    /// Should be called during application startup in development/staging environments.
    /// </summary>
    /// <param name="serviceProvider">Service provider for resolving dependencies</param>
    /// <returns>Task representing the async operation</returns>
    public static async Task EnsureDatabaseCreatedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            // Apply any pending migrations
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            // Log the error but don't fail startup
            var logger = scope.ServiceProvider.GetService<ILogger<ApplicationDbContext>>();
            logger?.LogError(ex, "An error occurred while migrating the database");
            throw;
        }
    }

    /// <summary>
    /// Seeds initial data into the database if it doesn't exist.
    /// Creates default admin user and roles for application setup.
    /// </summary>
    /// <param name="serviceProvider">Service provider for resolving dependencies</param>
    /// <param name="configuration">Configuration for default user settings</param>
    /// <returns>Task representing the async operation</returns>
    public static async Task SeedInitialDataAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Check if any users exist
        if (!await context.Users.AnyAsync())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();

            // Create default admin user
            var adminEmail = configuration.GetValue<string>("DefaultAdmin:Email");
            var adminPassword = configuration.GetValue<string>("DefaultAdmin:Password");

            if (adminEmail == null || adminPassword == null)
            {
                throw new Exception("Default admin missing in configuration.");
            }

            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                // Note: In production, admin should be required to set up 2FA immediately
                // For initial setup, we'll allow login without 2FA, but this should be addressed
                var logger = scope.ServiceProvider.GetService<ILogger<ApplicationDbContext>>();
                logger?.LogInformation("Default admin user created: {Email}", adminEmail);
            }
        }
    }
}
