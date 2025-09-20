using Microsoft.Extensions.DependencyInjection;

namespace MJAutomationCloud.Application;

/// <summary>
/// Application layer dependency injection configuration.
/// Registers application services following DDD principles.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all application services with the dependency injection container.
    /// Configures service lifetimes and implementations for application layer.
    /// </summary>
    /// <param name="services">Service collection for dependency registration</param>
    /// <returns>Service collection for method chaining</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register application services

        // Add other application services here as they are created
        // services.AddScoped<IUserManagementService, UserManagementService>();
        // services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
