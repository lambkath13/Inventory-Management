using InventoryApp.Models;

public interface IAuthService
{
    Task<(bool Succeeded, IEnumerable<string> Errors)> RegisterAsync(string email, string password, string displayName);
    Task<(bool Succeeded, string? Error)> LoginAsync(string email, string password, bool rememberMe);
    Task LogoutAsync();
    Task<(bool Ok, IEnumerable<string> Errors)> SetPasswordAsync(AppUser user, string newPassword);
}