using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MJAutomationCloud.Infrastructure.Entities;

namespace MJAutomationCloud.Infrastructure.Data;

/// <summary>
/// Application database context extending IdentityDbContext for ASP.NET Core Identity integration.
/// Manages all entity configurations and database operations following DDD principles.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    /// <summary>
    /// Initializes the database context with the provided options.
    /// </summary>
    /// <param name="options">Database context configuration options</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// DbSet for password reset tokens.
    /// Manages password recovery token lifecycle and validation.
    /// </summary>
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;

    /// <summary>
    /// Configures entity relationships and database schema using Fluent API.
    /// Applies domain constraints and database-specific configurations.
    /// </summary>
    /// <param name="builder">Model builder for entity configuration</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Apply base Identity configurations
        base.OnModelCreating(builder);

        // Configure ApplicationUser entity
        builder.Entity<ApplicationUser>(entity =>
        {
            // Configure properties with appropriate constraints
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("datetime('now')")
                .IsRequired();

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            entity.Property(e => e.FailedLoginAttempts)
                .HasDefaultValue(0)
                .IsRequired();

            // Configure indexes for performance
            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_ApplicationUser_Email");

            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_ApplicationUser_IsActive");

            entity.HasIndex(e => e.LastLoginAt)
                .HasDatabaseName("IX_ApplicationUser_LastLoginAt");
        });

        // Configure PasswordResetToken entity
        builder.Entity<PasswordResetToken>(entity =>
        {
            // Primary key configuration
            entity.HasKey(e => e.Id);

            // Configure properties
            entity.Property(e => e.Token)
                .HasMaxLength(64) // Accommodate hashed tokens
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("datetime('now')")
                .IsRequired();

            entity.Property(e => e.ExpiresAt)
                .IsRequired();

            entity.Property(e => e.IsUsed)
                .HasDefaultValue(false)
                .IsRequired();

            // Configure relationship with ApplicationUser
            entity.HasOne(e => e.User)
                .WithMany(u => u.PasswordResetTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_PasswordResetToken_ApplicationUser");

            // Configure indexes for performance and security
            entity.HasIndex(e => e.Token)
                .IsUnique()
                .HasDatabaseName("IX_PasswordResetToken_Token");

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_PasswordResetToken_UserId");

            entity.HasIndex(e => e.ExpiresAt)
                .HasDatabaseName("IX_PasswordResetToken_ExpiresAt");

            entity.HasIndex(e => new { e.IsUsed, e.ExpiresAt })
                .HasDatabaseName("IX_PasswordResetToken_IsUsed_ExpiresAt");
        });

        // Apply additional configurations if needed
        ApplyDomainConstraints(builder);
    }

    /// <summary>
    /// Applies domain-specific constraints and business rules to the database schema.
    /// Ensures data integrity at the database level.
    /// Note: SQLite has limited check constraint support, so we rely more on application-level validation.
    /// </summary>
    /// <param name="builder">Model builder for applying constraints</param>
    private static void ApplyDomainConstraints(ModelBuilder builder)
    {
        // SQLite doesn't support complex check constraints like SQL Server
        // Domain constraints are enforced at the application level
    }

    /// <summary>
    /// Override SaveChanges to apply domain-specific logic before persisting changes.
    /// Implements cross-cutting concerns like auditing and business rule validation.
    /// </summary>
    /// <returns>Number of affected rows</returns>
    public override int SaveChanges()
    {
        ApplyDomainLogic();
        return base.SaveChanges();
    }

    /// <summary>
    /// Async version of SaveChanges with domain logic application.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>Number of affected rows</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyDomainLogic();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Applies domain-specific business logic before saving changes.
    /// Ensures consistency and implements domain rules.
    /// </summary>
    private void ApplyDomainLogic()
    {
        var entries = ChangeTracker.Entries<ApplicationUser>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var user = entry.Entity;

            // Reset failed login attempts when user successfully logs in
            if (entry.Property(nameof(ApplicationUser.LastLoginAt)).IsModified && 
                user.LastLoginAt.HasValue)
            {
                user.FailedLoginAttempts = 0;
                user.LockedUntil = null;
            }
        }
    }
}
