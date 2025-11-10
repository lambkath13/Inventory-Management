using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InventoryApp.Services;
using Microsoft.AspNetCore.Identity;
using InventoryApp.Models;
using System.Security.Claims;
using InventoryApp.Dto;

namespace InventoryApp.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _accounts;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;

    public AuthController(IAuthService accounts, SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
    {
        _accounts = accounts;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [AllowAnonymous]
    [HttpGet("/auth/login")]
    public IActionResult Login(string? returnUrl = "/")
    {
        ViewBag.ReturnUrl = SafeLocal(returnUrl);
        return View(new Login());
    }

    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [HttpPost("/auth/login")]
    public async Task<IActionResult> LoginPost(Login log, string? returnUrl = "/")
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ReturnUrl = SafeLocal(returnUrl);
            return View("Login", log);
        }

        var (ok, err) = await _accounts.LoginAsync(log.Email, log.Password, log.RememberMe);
        if (!ok)
        {
            ModelState.AddModelError("", err ?? "Login failed");
            ViewBag.ReturnUrl = SafeLocal(returnUrl);
            return View("Login", log);
        }
        return LocalRedirect(SafeLocal(returnUrl));
    }

    [AllowAnonymous]
    [HttpGet("/auth/register")]
    public IActionResult Register(string? returnUrl = "/")
    {
        ViewBag.ReturnUrl = SafeLocal(returnUrl);
        return View(new Register());
    }

    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [HttpPost("/auth/register")]
    public async Task<IActionResult> RegisterPost(Register log, string? returnUrl = "/")
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ReturnUrl = SafeLocal(returnUrl);
            return View("Register", log);
        }

        var (ok, errs) = await _accounts.RegisterAsync(log.Email, log.Password, log.DisplayName);
        if (!ok)
        {
            foreach (var e in errs) ModelState.AddModelError("", e);
            ViewBag.ReturnUrl = SafeLocal(returnUrl);
            return View("Register", log);
        }
        return LocalRedirect(SafeLocal(returnUrl));
    }

    [ValidateAntiForgeryToken]
    [HttpPost("/auth/logout")]
    public async Task<IActionResult> Logout()
    {
        await _accounts.LogoutAsync();
        return Redirect("/");
    }

    [Authorize]
    [HttpGet("/auth/set-password")]
    public IActionResult SetPassword() => View(new SetPassword());

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost("/auth/set-password")]
    public async Task<IActionResult> SetPasswordPost(SetPassword log)
    {
        if (!ModelState.IsValid) return View("SetPassword", log);

        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction(nameof(Login));

        var (ok, errors) = await _accounts.SetPasswordAsync(user, log.NewPassword);
        if (!ok)
        {
            foreach (var e in errors) ModelState.AddModelError("", e);
            return View("SetPassword", log);
        }

        TempData["Msg"] = "Password has been set. You can now login with email and password.";
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [HttpPost("/auth/external")]
    public IActionResult ExternalLogin(string provider, string? returnUrl = "/")
    {
        if (string.IsNullOrWhiteSpace(provider)) return BadRequest("Provider is required.");
        var redirectUrl = Url.Action(nameof(ExternalCallback), "Auth", new { returnUrl = SafeLocal(returnUrl) })!;
        var props = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(props, provider);
    }

    [AllowAnonymous]
    [HttpGet("/auth/external-callback")]
    public async Task<IActionResult> ExternalCallback(string? returnUrl = "/")
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null) return LocalRedirect("/auth/login");

        var res = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
        if (res.Succeeded) return LocalRedirect(SafeLocal(returnUrl));

        var email = info.Principal.FindFirstValue(ClaimTypes.Email)
                    ?? info.Principal.Identity?.Name
                    ?? $"{info.LoginProvider}_{info.ProviderKey}@local";
        var displayName = info.Principal.Identity?.Name ?? email;

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new AppUser { UserName = email, Email = email, DisplayName = displayName, CreatedAt = DateTime.UtcNow };
            var createRes = await _userManager.CreateAsync(user);
            if (!createRes.Succeeded)
            {
                TempData["AuthError"] = string.Join(", ", createRes.Errors.Select(e => e.Description));
                return LocalRedirect("/auth/login");
            }
        }

        if (user.IsBlocked)
        {
            TempData["AuthError"] = "Your account is blocked.";
            return LocalRedirect("/auth/login");
        }

        var addLoginRes = await _userManager.AddLoginAsync(user, info); 
        await _signInManager.SignInAsync(user, isPersistent: false);
        return LocalRedirect(SafeLocal(returnUrl));
    }

    private static string SafeLocal(string? url)
        => string.IsNullOrWhiteSpace(url) ? "/" : (url!.StartsWith('/') ? url : "/");
}
