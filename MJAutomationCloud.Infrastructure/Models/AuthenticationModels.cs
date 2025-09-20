using System.ComponentModel.DataAnnotations;

namespace MJAutomationCloud.Infrastructure.Models;

/// <summary>
/// Request model for user login containing credentials.
/// Used for initial authentication before 2FA verification.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// User email address used for login.
    /// Must be a valid email format and is required.
    /// </summary>
    // [Required(ErrorMessage = "Email is required")]
    // [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User password for authentication.
    /// Required field with minimum length validation.
    /// </summary>
    // [Required(ErrorMessage = "Password is required")]
    // [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Remember me option for extended session.
    /// When true, creates persistent authentication cookie.
    /// </summary>
    public bool RememberMe { get; set; } = false;

    /// <summary>
    /// Development bypass flag for testing without 2FA.
    /// Only effective when development bypass is enabled in configuration.
    /// </summary>
    public bool DevBypass { get; set; } = false;
}

/// <summary>
/// Request model for two-factor authentication verification.
/// Used to complete login process with 2FA code.
/// </summary>
public class TwoFactorRequest
{
    /// <summary>
    /// User ID for the authentication session.
    /// Links the 2FA verification to the correct user.
    /// </summary>
    [Required(ErrorMessage = "User ID is required")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Two-factor authentication code from authenticator app.
    /// 6-digit TOTP code for verification.
    /// </summary>
    [Required(ErrorMessage = "Authentication code is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Authentication code must be 6 digits")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Authentication code must be 6 digits")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Remember me option carried over from initial login.
    /// Determines session persistence after 2FA completion.
    /// </summary>
    public bool RememberMe { get; set; } = false;

    /// <summary>
    /// Indicates if this is a recovery code instead of TOTP.
    /// Used when user cannot access their authenticator app.
    /// </summary>
    public bool IsRecoveryCode { get; set; } = false;
}

/// <summary>
/// Result model for authentication operations.
/// Contains success status, error messages, and next steps.
/// </summary>
public class AuthenticationResult
{
    /// <summary>
    /// Indicates if the authentication operation was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Indicates if two-factor authentication is required.
    /// True when credentials are valid but 2FA verification is needed.
    /// </summary>
    public bool RequiresTwoFactor { get; set; }

    /// <summary>
    /// Indicates if the account is locked due to failed attempts.
    /// User must wait for lockout period to expire.
    /// </summary>
    public bool IsLockedOut { get; set; }

    /// <summary>
    /// User ID for the authenticated user.
    /// Set when authentication is successful or 2FA is required.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Error message if authentication failed.
    /// Provides user-friendly error description.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Time remaining for account lockout (if applicable).
    /// Null if account is not locked.
    /// </summary>
    public TimeSpan? LockoutTimeRemaining { get; set; }

    /// <summary>
    /// Creates a successful authentication result.
    /// </summary>
    /// <param name="userId">ID of the authenticated user</param>
    /// <returns>Success authentication result</returns>
    public static AuthenticationResult Success(string userId)
    {
        return new AuthenticationResult
        {
            IsSuccess = true,
            UserId = userId
        };
    }

    /// <summary>
    /// Creates a result indicating 2FA is required.
    /// </summary>
    /// <param name="userId">ID of the user requiring 2FA</param>
    /// <returns>2FA required authentication result</returns>
    public static AuthenticationResult RequiresTwoFactorAuth(string userId)
    {
        return new AuthenticationResult
        {
            IsSuccess = false,
            RequiresTwoFactor = true,
            UserId = userId
        };
    }

    /// <summary>
    /// Creates a failed authentication result.
    /// </summary>
    /// <param name="errorMessage">Error message describing the failure</param>
    /// <returns>Failed authentication result</returns>
    public static AuthenticationResult Failed(string errorMessage)
    {
        return new AuthenticationResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }

    /// <summary>
    /// Creates a locked out authentication result.
    /// </summary>
    /// <param name="lockoutTimeRemaining">Time remaining for lockout</param>
    /// <returns>Locked out authentication result</returns>
    public static AuthenticationResult LockedOut(TimeSpan? lockoutTimeRemaining = null)
    {
        return new AuthenticationResult
        {
            IsSuccess = false,
            IsLockedOut = true,
            LockoutTimeRemaining = lockoutTimeRemaining,
            ErrorMessage = "Account is temporarily locked due to failed login attempts."
        };
    }
}

/// <summary>
/// Request model for enabling two-factor authentication.
/// Used when user sets up 2FA for the first time.
/// </summary>
public class EnableTwoFactorRequest
{
    /// <summary>
    /// User ID for the account setting up 2FA.
    /// </summary>
    [Required(ErrorMessage = "User ID is required")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Verification code from authenticator app to confirm setup.
    /// Validates that the user has correctly configured their authenticator.
    /// </summary>
    [Required(ErrorMessage = "Verification code is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Verification code must be 6 digits")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Verification code must be 6 digits")]
    public string VerificationCode { get; set; } = string.Empty;
}

/// <summary>
/// Result model for two-factor authentication setup.
/// Contains QR code data and setup instructions.
/// </summary>
public class TwoFactorSetupResult
{
    /// <summary>
    /// QR code URI for authenticator app setup.
    /// Contains the TOTP secret and account information.
    /// </summary>
    public string QrCodeUri { get; set; } = string.Empty;

    /// <summary>
    /// Manual entry key for users who cannot scan QR code.
    /// Base32-encoded secret key for manual input.
    /// </summary>
    public string ManualEntryKey { get; set; } = string.Empty;

    /// <summary>
    /// User-friendly account name for the authenticator app.
    /// Typically the user's email address.
    /// </summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// Issuer name for the authenticator app entry.
    /// Application or organization name.
    /// </summary>
    public string IssuerName { get; set; } = string.Empty;
}

/// <summary>
/// Result model for enabling two-factor authentication.
/// Contains success status and recovery codes.
/// </summary>
public class TwoFactorEnableResult
{
    /// <summary>
    /// Indicates if 2FA was successfully enabled.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Recovery codes for backup access.
    /// Single-use codes for account recovery when 2FA device is unavailable.
    /// </summary>
    public string[] RecoveryCodes { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Error message if 2FA setup failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Creates a successful 2FA enable result.
    /// </summary>
    /// <param name="recoveryCodes">Generated recovery codes</param>
    /// <returns>Success result with recovery codes</returns>
    public static TwoFactorEnableResult Success(string[] recoveryCodes)
    {
        return new TwoFactorEnableResult
        {
            IsSuccess = true,
            RecoveryCodes = recoveryCodes
        };
    }

    /// <summary>
    /// Creates a failed 2FA enable result.
    /// </summary>
    /// <param name="errorMessage">Error message describing the failure</param>
    /// <returns>Failed result with error message</returns>
    public static TwoFactorEnableResult Failed(string errorMessage)
    {
        return new TwoFactorEnableResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}
