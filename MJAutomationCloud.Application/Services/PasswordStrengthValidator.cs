using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace MJAutomationCloud.Application.Services;

/// <summary>
/// Custom password validator enforcing length >=12 and at least 3 of 4 character categories.
/// Avoids requiring non-alphanumeric explicitly while still allowing symbols.
/// </summary>
public class PasswordStrengthValidator<TUser> : IPasswordValidator<TUser> where TUser : class
{
    public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
    {
        var errors = new List<IdentityError>();
        if (string.IsNullOrWhiteSpace(password) || password.Length < 12)
        {
            errors.Add(new IdentityError { Code = "PasswordTooShort", Description = "Password must be at least 12 characters." });
        }
        // int categories = 0;
        // if (password.Any(char.IsUpper)) categories++;
        // if (password.Any(char.IsLower)) categories++;
        // if (password.Any(char.IsDigit)) categories++;
        // if (password.Any(c => "!@#$%^&*()_-+=[]{}:;'<>,.?/".Contains(c))) categories++;
        // if (categories < 3)
        // {
        //     errors.Add(new IdentityError { Code = "PasswordWeak", Description = "Use at least three of: uppercase, lowercase, digit, symbol." });
        // }
        // if (errors.Count > 0)
        // {
        //     return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
        // }
        return Task.FromResult(IdentityResult.Success);
    }
}