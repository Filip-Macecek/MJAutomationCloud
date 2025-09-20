namespace MJAutomationCloud.Infrastructure.Entities;

/// <summary>
/// Entity representing a password reset token for secure password recovery flow.
/// Implements token-based password reset with expiration and single-use constraints.
/// </summary>
public class PasswordResetToken
{
    /// <summary>
    /// Unique identifier for the password reset token.
    /// Primary key for the entity.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key reference to the user requesting password reset.
    /// Links the token to a specific user account.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Cryptographically secure token for password reset verification.
    /// Generated using secure random number generation.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the token was created.
    /// Used for expiration calculations and auditing.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the token expires and becomes invalid.
    /// Typically set to 1-24 hours from creation time.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Indicates whether the token has been used for password reset.
    /// Prevents token reuse for security.
    /// </summary>
    public bool IsUsed { get; set; } = false;

    /// <summary>
    /// Timestamp when the token was used (if applicable).
    /// Null if token hasn't been used yet.
    /// </summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>
    /// Navigation property to the associated user.
    /// EF Core relationship configuration.
    /// </summary>
    public virtual ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Checks if the token is currently valid (not expired and not used).
    /// Business logic for token validation.
    /// </summary>
    public bool IsValid => !IsUsed && DateTime.UtcNow < ExpiresAt;
}
