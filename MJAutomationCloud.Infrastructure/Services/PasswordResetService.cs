using Microsoft.EntityFrameworkCore;
using MJAutomationCloud.Infrastructure.Data;
using System.Security.Cryptography;
using System.Text;
using MJAutomationCloud.Infrastructure.Common;
using MJAutomationCloud.Infrastructure.Entities;

namespace MJAutomationCloud.Infrastructure.Services;

/// <summary>
/// Service for managing password reset functionality.
/// Implements secure token-based password recovery flow according to domain specifications.
/// </summary>
public class PasswordResetService
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes the password reset service with required dependencies.
    /// </summary>
    /// <param name="context">Database context for data operations</param>
    public PasswordResetService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Generates a secure password reset token for the specified user.
    /// Creates a cryptographically secure token with expiration time.
    /// </summary>
    /// <param name="userId">ID of the user requesting password reset</param>
    /// <returns>Password reset token entity</returns>
    /// <exception cref="ArgumentException">Thrown when user ID is invalid</exception>
    public async Task<PasswordResetToken> GeneratePasswordResetTokenAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

        // Verify user exists and is active
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        if (user == null)
            throw new ArgumentException("User not found or inactive", nameof(userId));

        // Invalidate any existing active tokens for this user
        await InvalidateExistingTokensAsync(userId);

        // Generate cryptographically secure token
        var token = GenerateSecureToken();
        var hashedToken = HashToken(token);

        // Create password reset token entity
        var resetToken = new PasswordResetToken
        {
            UserId = userId,
            Token = hashedToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(DomainConstants.Authentication.PasswordResetTokenExpirationHours),
            IsUsed = false
        };

        // Store in database
        _context.PasswordResetTokens.Add(resetToken);
        await _context.SaveChangesAsync();

        // Return token with plain text for email sending (will be hashed when stored)
        resetToken.Token = token; // Temporarily set plain text for return
        return resetToken;
    }

    /// <summary>
    /// Validates a password reset token and returns the associated token entity if valid.
    /// Performs comprehensive validation including expiration and usage status.
    /// </summary>
    /// <param name="token">Plain text token to validate</param>
    /// <returns>Valid password reset token entity, or null if invalid</returns>
    public async Task<PasswordResetToken?> ValidateTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var hashedToken = HashToken(token);

        // Find token in database with user information
        var resetToken = await _context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t =>
                t.Token == hashedToken &&
                !t.IsUsed &&
                t.ExpiresAt > DateTime.UtcNow &&
                t.User.IsActive);

        return resetToken;
    }

    /// <summary>
    /// Marks a password reset token as used to prevent reuse.
    /// Updates the token status and timestamp in the database.
    /// </summary>
    /// <param name="tokenId">ID of the token to mark as used</param>
    public async Task MarkTokenAsUsedAsync(Guid tokenId)
    {
        var token = await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.Id == tokenId);

        if (token != null)
        {
            token.IsUsed = true;
            token.UsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Cleans up expired password reset tokens from the database.
    /// Should be called periodically to maintain database hygiene.
    /// </summary>
    public async Task CleanupExpiredTokensAsync()
    {
        var expiredTokens = await _context.PasswordResetTokens
            .Where(t => t.ExpiresAt < DateTime.UtcNow || t.IsUsed)
            .Where(t => t.CreatedAt < DateTime.UtcNow.AddDays(-30)) // Keep for 30 days for auditing
            .ToListAsync();

        if (expiredTokens.Any())
        {
            _context.PasswordResetTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Invalidates all existing active password reset tokens for a user.
    /// Called when generating a new token to ensure only one active token per user.
    /// </summary>
    /// <param name="userId">ID of the user whose tokens should be invalidated</param>
    private async Task InvalidateExistingTokensAsync(string userId)
    {
        var existingTokens = await _context.PasswordResetTokens
            .Where(t => t.UserId == userId && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var token in existingTokens)
        {
            token.IsUsed = true;
            token.UsedAt = DateTime.UtcNow;
        }

        if (existingTokens.Any())
        {
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Generates a cryptographically secure random token.
    /// Uses RNGCryptoServiceProvider for secure random number generation.
    /// </summary>
    /// <returns>Base64-encoded secure random token</returns>
    private static string GenerateSecureToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var tokenBytes = new byte[DomainConstants.Authentication.PasswordResetTokenLength];
        rng.GetBytes(tokenBytes);
        return Convert.ToBase64String(tokenBytes);
    }

    /// <summary>
    /// Hashes a token using SHA-256 for secure storage.
    /// Prevents token exposure in case of database compromise.
    /// </summary>
    /// <param name="token">Plain text token to hash</param>
    /// <returns>SHA-256 hash of the token</returns>
    private static string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hashedBytes);
    }
}
