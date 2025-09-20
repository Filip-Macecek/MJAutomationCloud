using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MJAutomationCloud.Infrastructure.Common;
using MJAutomationCloud.Infrastructure.Entities;
using MJAutomationCloud.Infrastructure.Interfaces;
using MJAutomationCloud.Infrastructure.Models;

namespace MJAutomationCloud.Infrastructure.Services;

/// <summary>
/// Implementation of authentication services with 2FA support and development bypass.
/// Handles user login, 2FA verification, and security policies according to domain rules.
/// Note: This is a simplified version that focuses on credential validation and 2FA.
/// The actual sign-in process is handled by the Infrastructure layer.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticationService> _logger;

    /// <summary>
    /// Initializes the authentication service with required dependencies.
    /// </summary>
    public AuthenticationService(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger<AuthenticationService> logger)
    {
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user with email/username and password.
    /// Supports development bypass for testing without 2FA when configured.
    /// </summary>
    public async Task<AuthenticationResult> LoginAsync(LoginRequest request)
    {
        try
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Login attempt for non-existent or inactive user: {Email}", request.Email);
                return AuthenticationResult.Failed("Invalid email or password.");
            }

            // Check if account is locked
            if (await _userManager.IsLockedOutAsync(user))
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                var timeRemaining = lockoutEnd?.Subtract(DateTimeOffset.UtcNow);

                _logger.LogWarning("Login attempt for locked account: {Email}", request.Email);
                return AuthenticationResult.LockedOut(timeRemaining);
            }

            // Validate password
            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
            {
                // Increment failed login attempts
                await _userManager.AccessFailedAsync(user);

                _logger.LogWarning("Invalid password for user: {Email}", request.Email);
                return AuthenticationResult.Failed("Invalid email or password.");
            }

            // Check development bypass configuration
            var allowDevBypass = _configuration.GetValue<bool>(DomainConstants.Authentication.DevBypassConfigKey);
            var isDevelopment = _configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development";

            // If development bypass is enabled and requested, skip 2FA
            if (isDevelopment && allowDevBypass && request.DevBypass)
            {
                _logger.LogInformation("Development bypass used for user: {Email}", request.Email);

                // For development bypass, we still validate but return success immediately
                // The actual sign-in will be handled by the Infrastructure layer
                await UpdateLastLoginAsync(user);

                return AuthenticationResult.Success(user.Id);
            }

            // Check if 2FA is enabled for the user
            var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            if (!isTwoFactorEnabled)
            {
                _logger.LogWarning("Login attempt for user without 2FA: {Email}", request.Email);
                return AuthenticationResult.Failed("Two-factor authentication is required for all accounts.");
            }

            // Reset failed login attempts on successful credential validation
            await _userManager.ResetAccessFailedCountAsync(user);

            // Return result indicating 2FA is required
            _logger.LogInformation("2FA required for user: {Email}", request.Email);
            return AuthenticationResult.RequiresTwoFactorAuth(user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Email}", request.Email);
            return AuthenticationResult.Failed("An error occurred during login. Please try again.");
        }
    }

    /// <summary>
    /// Verifies a two-factor authentication code and completes the login process.
    /// Supports both TOTP codes and recovery codes.
    /// </summary>
    public async Task<AuthenticationResult> VerifyTwoFactorAsync(TwoFactorRequest request)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("2FA verification attempt for non-existent or inactive user: {UserId}", request.UserId);
                return AuthenticationResult.Failed("Invalid verification request.");
            }

            bool verificationResult;

            if (request.IsRecoveryCode)
            {
                // Verify recovery code
                var recoveryResult = await _userManager.RedeemTwoFactorRecoveryCodeAsync(user, request.Code);
                verificationResult = recoveryResult.Succeeded;

                if (verificationResult)
                {
                    _logger.LogInformation("Recovery code used for user: {UserId}", request.UserId);
                }
                else
                {
                    _logger.LogWarning("Invalid recovery code for user: {UserId}", request.UserId);
                }
            }
            else
            {
                // Verify TOTP code
                verificationResult = await _userManager.VerifyTwoFactorTokenAsync(
                    user,
                    _userManager.Options.Tokens.AuthenticatorTokenProvider,
                    request.Code);

                if (verificationResult)
                {
                    _logger.LogInformation("TOTP code verified for user: {UserId}", request.UserId);
                }
                else
                {
                    _logger.LogWarning("Invalid TOTP code for user: {UserId}", request.UserId);
                }
            }

            if (!verificationResult)
            {
                // Increment failed attempts for 2FA as well
                await _userManager.AccessFailedAsync(user);
                return AuthenticationResult.Failed("Invalid authentication code.");
            }

            // 2FA verification successful - the actual sign-in will be handled by Infrastructure layer
            await UpdateLastLoginAsync(user);

            _logger.LogInformation("User successfully verified 2FA: {UserId}", request.UserId);
            return AuthenticationResult.Success(user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during 2FA verification for user: {UserId}", request.UserId);
            return AuthenticationResult.Failed("An error occurred during verification. Please try again.");
        }
    }

    /// <summary>
    /// Logs out the current user and clears their authentication session.
    /// Note: The actual sign-out process is handled by the Infrastructure layer.
    /// </summary>
    public async Task LogoutAsync(string userId)
    {
        try
        {
            // In DDD, the application layer focuses on business logic
            // The actual sign-out is handled by the Infrastructure layer
            _logger.LogInformation("User logout requested: {UserId}", userId);
            await Task.CompletedTask; // Placeholder for any business logic if needed
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user: {UserId}", userId);
        }
    }

    /// <summary>
    /// Checks if two-factor authentication is enabled for a user.
    /// </summary>
    public async Task<bool> IsTwoFactorEnabledAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user != null && await _userManager.GetTwoFactorEnabledAsync(user);
    }

    /// <summary>
    /// Generates QR code data for setting up authenticator app 2FA.
    /// Creates TOTP setup information for user enrollment.
    /// </summary>
    public async Task<TwoFactorSetupResult> GenerateTwoFactorSetupAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException("User not found", nameof(userId));
        }

        // Reset authenticator key to generate a new one
        await _userManager.ResetAuthenticatorKeyAsync(user);

        // Get the new authenticator key
        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(key))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            key = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        var issuer = _configuration["Authentication:Issuer"] ?? "MJAutomationCloud";
        var qrCodeUri = GenerateQrCodeUri(user.Email!, key!, issuer);

        return new TwoFactorSetupResult
        {
            QrCodeUri = qrCodeUri,
            ManualEntryKey = FormatKey(key!),
            AccountName = user.Email!,
            IssuerName = issuer
        };
    }

    /// <summary>
    /// Enables two-factor authentication after verifying the setup code.
    /// Validates authenticator app setup and generates recovery codes.
    /// </summary>
    public async Task<TwoFactorEnableResult> EnableTwoFactorAsync(EnableTwoFactorRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return TwoFactorEnableResult.Failed("User not found.");
        }

        // Verify the code from the authenticator app
        var isValidCode = await _userManager.VerifyTwoFactorTokenAsync(
            user,
            _userManager.Options.Tokens.AuthenticatorTokenProvider,
            request.VerificationCode);

        if (!isValidCode)
        {
            _logger.LogWarning("Invalid 2FA setup code for user: {UserId}", request.UserId);
            return TwoFactorEnableResult.Failed("Invalid verification code.");
        }

        // Enable 2FA for the user
        await _userManager.SetTwoFactorEnabledAsync(user, true);

        // Generate recovery codes
        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(
            user,
            DomainConstants.TwoFactorAuth.RecoveryCodesCount);

        _logger.LogInformation("2FA enabled for user: {UserId}", request.UserId);
        return TwoFactorEnableResult.Success(recoveryCodes?.ToArray() ?? Array.Empty<string>());
    }

    /// <summary>
    /// Generates new recovery codes for two-factor authentication backup.
    /// </summary>
    public async Task<string[]> GenerateRecoveryCodesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException("User not found", nameof(userId));
        }

        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(
            user,
            DomainConstants.TwoFactorAuth.RecoveryCodesCount);

        _logger.LogInformation("New recovery codes generated for user: {UserId}", userId);
        return recoveryCodes?.ToArray() ?? Array.Empty<string>();
    }

    /// <summary>
    /// Validates user credentials without completing the login process.
    /// </summary>
    public async Task<bool> ValidateCredentialsAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !user.IsActive)
        {
            return false;
        }

        return await _userManager.CheckPasswordAsync(user, password);
    }

    /// <summary>
    /// Updates the user's last login timestamp.
    /// </summary>
    private async Task UpdateLastLoginAsync(ApplicationUser user)
    {
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
    }

    /// <summary>
    /// Generates a QR code URI for TOTP setup.
    /// </summary>
    private static string GenerateQrCodeUri(string email, string key, string issuer)
    {
        const string authenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
        return string.Format(
            authenticatorUriFormat,
            UrlEncoder.Default.Encode(issuer),
            UrlEncoder.Default.Encode(email),
            key);
    }

    /// <summary>
    /// Formats the authenticator key for manual entry (groups of 4 characters).
    /// </summary>
    private static string FormatKey(string key)
    {
        var result = new System.Text.StringBuilder();
        int currentPosition = 0;

        while (currentPosition + 4 < key.Length)
        {
            result.Append(key.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }

        if (currentPosition < key.Length)
        {
            result.Append(key.AsSpan(currentPosition));
        }

        return result.ToString().ToUpperInvariant();
    }
}
