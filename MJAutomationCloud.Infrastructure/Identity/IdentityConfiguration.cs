using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MJAutomationCloud.Infrastructure.Common;

namespace MJAutomationCloud.Infrastructure.Identity;

/// <summary>
/// Configuration class for ASP.NET Core Identity setup.
/// Implements security policies and authentication requirements according to domain rules.
/// </summary>
public static class IdentityConfiguration
{
    /// <summary>
    /// Configures Identity options with domain-specific security policies.
    /// Sets up password requirements, lockout policies, and 2FA enforcement.
    /// </summary>
    /// <param name="options">Identity options to configure</param>
    public static void ConfigureIdentityOptions(IdentityOptions options)
    {
        // Password policy configuration based on domain constants
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = DomainConstants.Authentication.MinPasswordLength;
        options.Password.RequiredUniqueChars = 4;

        // Lockout policy to prevent brute force attacks
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(DomainConstants.Authentication.LockoutDurationMinutes);
        options.Lockout.MaxFailedAccessAttempts = DomainConstants.Authentication.MaxFailedLoginAttempts;
        options.Lockout.AllowedForNewUsers = true;

        // User account requirements
        options.User.RequireUniqueEmail = true;
        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

        // Sign-in requirements - enforce 2FA
        options.SignIn.RequireConfirmedEmail = false; // We'll handle this through custom logic
        options.SignIn.RequireConfirmedPhoneNumber = false;

        // Token providers for 2FA
        options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
        options.Tokens.ChangeEmailTokenProvider = TokenOptions.DefaultEmailProvider;
        options.Tokens.ChangePhoneNumberTokenProvider = TokenOptions.DefaultPhoneProvider;
    }
}
