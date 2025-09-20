namespace MJAutomationCloud.Infrastructure.Common;

/// <summary>
/// Domain-level constants for authentication and security policies.
/// Centralized location for business rules and constraints.
/// </summary>
public static class DomainConstants
{
    /// <summary>
    /// Authentication-related constants and policies.
    /// </summary>
    public static class Authentication
    {
        /// <summary>
        /// Maximum number of failed login attempts before account lockout.
        /// Security measure to prevent brute force attacks.
        /// </summary>
        public const int MaxFailedLoginAttempts = 5;

        /// <summary>
        /// Duration (in minutes) for account lockout after max failed attempts.
        /// Temporary lockout to slow down attack attempts.
        /// </summary>
        public const int LockoutDurationMinutes = 15;

        /// <summary>
        /// Duration (in hours) for password reset token validity.
        /// Balance between security and user convenience.
        /// </summary>
        public const int PasswordResetTokenExpirationHours = 24;

        /// <summary>
        /// Length of generated password reset tokens.
        /// Cryptographically secure random token length.
        /// </summary>
        public const int PasswordResetTokenLength = 32;

        /// <summary>
        /// Minimum password length requirement.
        /// Enforced at domain level for security policy.
        /// </summary>
        public const int MinPasswordLength = 8;

        /// <summary>
        /// Development mode bypass flag configuration key.
        /// Allows testing without 2FA in development environment.
        /// </summary>
        public const string DevBypassConfigKey = "Authentication:AllowDevBypass";
    }

    /// <summary>
    /// Two-Factor Authentication related constants.
    /// </summary>
    public static class TwoFactorAuth
    {
        /// <summary>
        /// TOTP (Time-based One-Time Password) step size in seconds.
        /// Standard 30-second window for authenticator apps.
        /// </summary>
        public const int TotpStepSeconds = 30;

        /// <summary>
        /// Number of previous/next TOTP codes to accept for clock drift.
        /// Allows for minor time synchronization issues.
        /// </summary>
        public const int TotpTolerance = 1;

        /// <summary>
        /// Length of backup recovery codes.
        /// Single-use codes for 2FA recovery scenarios.
        /// </summary>
        public const int RecoveryCodeLength = 8;

        /// <summary>
        /// Number of recovery codes to generate per user.
        /// Provides multiple backup options for account recovery.
        /// </summary>
        public const int RecoveryCodesCount = 10;
    }
}
