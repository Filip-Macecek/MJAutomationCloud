using MJAutomationCloud.Infrastructure.Models;

namespace MJAutomationCloud.Infrastructure.Interfaces;

/// <summary>
/// Interface for authentication services following DDD application layer patterns.
/// Defines the contract for user authentication operations with 2FA support.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user with email/username and password.
    /// Returns authentication result with 2FA requirements if applicable.
    /// </summary>
    /// <param name="request">Login request containing credentials</param>
    /// <returns>Authentication result with success status and 2FA requirements</returns>
    Task<AuthenticationResult> LoginAsync(LoginRequest request);

    /// <summary>
    /// Verifies a two-factor authentication code for completing login.
    /// Completes the authentication process after initial credential validation.
    /// </summary>
    /// <param name="request">2FA verification request</param>
    /// <returns>Authentication result with final login status</returns>
    Task<AuthenticationResult> VerifyTwoFactorAsync(TwoFactorRequest request);

    /// <summary>
    /// Logs out the current user and invalidates their session.
    /// Clears authentication cookies and security tokens.
    /// </summary>
    /// <param name="userId">ID of the user to log out</param>
    /// <returns>Task representing the async operation</returns>
    Task LogoutAsync(string userId);

    /// <summary>
    /// Checks if a user has two-factor authentication enabled and configured.
    /// Used to determine authentication flow requirements.
    /// </summary>
    /// <param name="userId">ID of the user to check</param>
    /// <returns>True if 2FA is enabled and configured, false otherwise</returns>
    Task<bool> IsTwoFactorEnabledAsync(string userId);

    /// <summary>
    /// Generates QR code data for setting up authenticator app 2FA.
    /// Returns the data needed to display QR code for TOTP setup.
    /// </summary>
    /// <param name="userId">ID of the user setting up 2FA</param>
    /// <returns>QR code setup information</returns>
    Task<TwoFactorSetupResult> GenerateTwoFactorSetupAsync(string userId);

    /// <summary>
    /// Enables two-factor authentication for a user after verifying setup code.
    /// Validates the authenticator app setup and activates 2FA.
    /// </summary>
    /// <param name="request">2FA enable request with verification code</param>
    /// <returns>Result indicating success and recovery codes</returns>
    Task<TwoFactorEnableResult> EnableTwoFactorAsync(EnableTwoFactorRequest request);

    /// <summary>
    /// Generates new recovery codes for two-factor authentication backup.
    /// Replaces existing recovery codes with new ones.
    /// </summary>
    /// <param name="userId">ID of the user requesting new recovery codes</param>
    /// <returns>New recovery codes for the user</returns>
    Task<string[]> GenerateRecoveryCodesAsync(string userId);

    /// <summary>
    /// Validates user credentials without completing the login process.
    /// Used for credential verification before 2FA challenge.
    /// </summary>
    /// <param name="email">User email or username</param>
    /// <param name="password">User password</param>
    /// <returns>True if credentials are valid, false otherwise</returns>
    Task<bool> ValidateCredentialsAsync(string email, string password);
}
