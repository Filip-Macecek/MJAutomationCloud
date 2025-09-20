using Microsoft.AspNetCore.Identity;

namespace MJAutomationCloud.Infrastructure.Entities;

/// <summary>
/// Custom user entity extending IdentityUser for application-specific user properties.
/// Represents the domain aggregate root for user authentication and profile information.
/// </summary>
public class ApplicationUser : IdentityUser
{


    /// <summary>
    /// Timestamp when the user was created in the system.
    /// Used for auditing and account management.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp of the user's last login.
    /// Used for security monitoring and session management.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Indicates if the user account is currently active.
    /// Inactive users cannot log in regardless of credentials.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Number of failed login attempts.
    /// Used for account lockout protection and security monitoring.
    /// </summary>
    public int FailedLoginAttempts { get; set; } = 0;

    /// <summary>
    /// Timestamp when the account was locked due to failed attempts.
    /// Null if account is not currently locked.
    /// </summary>
    public DateTime? LockedUntil { get; set; }

    /// <summary>
    /// Collection of password reset tokens for this user.
    /// Navigation property for EF Core relationship.
    /// </summary>
    public virtual ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
}
