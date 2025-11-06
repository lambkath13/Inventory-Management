using InventoryApp.Models;
using Microsoft.AspNetCore.Identity;

namespace InventoryApp.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    public AuthService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<(bool Succeeded, IEnumerable<string> Errors)> RegisterAsync(string email, string password, string displayName)
    {
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            DisplayName = displayName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
            await _signInManager.SignInAsync(user, isPersistent: false);

        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }

    public async Task<(bool Succeeded, string? Error)> LoginAsync(string email, string password, bool rememberMe)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null) return (false, "Invalid email or password.");

        if (user.IsBlocked) return (false, "Your account is blocked.");

        if (string.IsNullOrEmpty(user.PasswordHash))
            return (false, "This account doesn't have a local password. Sign in with Google or set a password.");

        var res = await _signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: false);
        if (!res.Succeeded) return (false, "Invalid email or password.");

        return (true, null);
    }

    public async Task LogoutAsync() => await _signInManager.SignOutAsync();

    public async Task<(bool Ok, IEnumerable<string> Errors)> SetPasswordAsync(AppUser user, string newPassword)
    {
        if (!string.IsNullOrEmpty(user.PasswordHash))
            return (false, new[] { "You already have a local password." });

        var result = await _userManager.AddPasswordAsync(user, newPassword);
        return (result.Succeeded, result.Succeeded ? Array.Empty<string>() : result.Errors.Select(e => e.Description));
    }
}
